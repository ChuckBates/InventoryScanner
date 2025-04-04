using Docker.DotNet.Models;
using Docker.DotNet;
using InventoryScannerCore.Settings;
using RabbitMQ.Client;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System;

namespace InventoryScannerCore.IntegrationTests
{
    public class RabbitTestContext : IDisposable
    {
        private IConnection? connection;
        private IModel? channel;
        private readonly RabbitMqSettings rabbitSettings;

        public RabbitTestContext(RabbitMqSettings rabbitSettings)
        {
            this.rabbitSettings = rabbitSettings;
            BuildConnection();

            PurgeQueue(rabbitSettings.FetchInventoryMetadataQueueName).Wait(3000);
        }

        private void BuildConnection()
        {
            var factory = new ConnectionFactory
            {
                HostName = rabbitSettings.HostName,
                Port = rabbitSettings.AmqpPort,
                UserName = rabbitSettings.UserName,
                Password = rabbitSettings.Password,
                RequestedConnectionTimeout = TimeSpan.FromSeconds(rabbitSettings.ConnectionTimeout),
            };

            connection = factory.CreateConnection();
            channel = connection.CreateModel();

            channel.ExchangeDeclare(
                exchange: rabbitSettings.FetchInventoryMetadataExchangeName,
                type: ExchangeType.Fanout,
                durable: true,
                autoDelete: false);

            channel.QueueDeclare(
                queue: rabbitSettings.FetchInventoryMetadataQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            channel.QueueBind(
                queue: rabbitSettings.FetchInventoryMetadataQueueName,
                exchange: rabbitSettings.FetchInventoryMetadataExchangeName,
                routingKey: "");
        }

        public async Task PurgeQueue(string queueName, bool rebuildConnection = false)
        {
            if (rebuildConnection)
            {
                connection?.Close();
                channel?.Close();
                BuildConnection();
            }

            channel.QueuePurge(queueName);
            await Task.Delay(300);
        }

        public async Task<List<T>> ReadMessages<T>(string queueName, int expectedCount, int maxAttempts = 30, int delayMs = 100)
        {
            var messages = new List<T>();
            var attempts = 0;

            while (messages.Count < expectedCount && attempts < maxAttempts)
            {
                while (true)
                {
                    var result = channel.BasicGet(queueName, autoAck: true);
                    if (result == null) break;

                    var body = Encoding.UTF8.GetString(result.Body.ToArray());
                    var message = JsonSerializer.Deserialize<T>(body);
                    if (message != null)
                        messages.Add(message);
                }

                if (messages.Count < expectedCount)
                    await Task.Delay(delayMs);

                attempts++;
            }

            return messages;
        }

        public async Task RestartRabbitMqAsync(
            string containerName = "rabbitmq",
            string host = "localhost",
            int amqpPort = 5672,
            int managementPort = 15672,
            string username = "guest",
            string password = "guest",
            int retryDelayMs = 500,
            int timeoutSeconds = 20,
            int waitDelayMs = 0)
        {
            await ShutdownRabbitMqAsync(containerName);
            await StartRabbitMqAsync(containerName, host, amqpPort, managementPort, username, password, retryDelayMs, timeoutSeconds);
        }

        public async Task StartRabbitMqAsync(
            string containerName = "rabbitmq",
            string host = "localhost", 
            int amqpPort = 5672, 
            int managementPort = 15672, 
            string username = "guest", 
            string password = "guest", 
            int retryDelayMs = 500, 
            int timeoutSeconds = 20)
        {

            using (var docker = new DockerClientConfiguration().CreateClient())
            {
                try
                {
                    Console.WriteLine($"Starting RabbitMQ container '{containerName}'...");
                    await docker.Containers.StartContainerAsync(containerName, new ContainerStartParameters());
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"Failed to start RabbitMQ container '{containerName}' via Docker.",
                        ex);
                }
            };

            await WaitForRabbitMqManagementApiAsync(host, managementPort, username, password, retryDelayMs, timeoutSeconds);
            await WaitForAmqpPortReadyAsync(host, amqpPort);
        }

        public async Task ShutdownRabbitMqAsync(string containerName = "rabbitmq")
        {
            using (var docker = new DockerClientConfiguration().CreateClient())
            {
                try
                {
                    Console.WriteLine($"Stopping RabbitMQ container '{containerName}'...");
                    await docker.Containers.StopContainerAsync(containerName, new ContainerStopParameters());
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"Failed to stop RabbitMQ container '{containerName}' via Docker.",
                        ex);
                }
            };
        }

        public static async Task WaitForRabbitMqManagementApiAsync(
            string host = "localhost",
            int port = 15672,
            string username = "guest",
            string password = "guest",
            int retryDelayMs = 500,
            int timeoutSeconds = 20)
        {
            using var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Basic",
                    Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}")));

            var endpoint = $"http://{host}:{port}/api/overview";
            var start = DateTime.UtcNow;

            Console.WriteLine($"Waiting for RabbitMQ management API at {endpoint}...");

            while ((DateTime.UtcNow - start).TotalSeconds < timeoutSeconds)
            {
                try
                {
                    var response = await httpClient.GetAsync(endpoint);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("RabbitMQ management API is healthy.");
                        return;
                    }

                    Console.WriteLine($"Management API returned {response.StatusCode} - retrying...");
                }
                catch (HttpRequestException hre)
                {
                    Console.WriteLine($"HTTP request failed: {hre.Message} - retrying...");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unknown error: {ex.Message} - retrying...");
                }

                await Task.Delay(retryDelayMs);
            }

            throw new TimeoutException($"RabbitMQ management API at {endpoint} was not healthy after {timeoutSeconds} seconds.");
        }

        public static async Task WaitForAmqpPortReadyAsync(string host, int port, int timeoutMs = 15000, int delayMs = 500)
        {
            Console.WriteLine($"Waiting for full AMQP handshake at {host}:{port}...");

            var start = DateTime.UtcNow;
            var preamble = new byte[] { (byte)'A', (byte)'M', (byte)'Q', (byte)'P', 0, 0, 9, 1 };

            while ((DateTime.UtcNow - start).TotalMilliseconds < timeoutMs)
            {
                try
                {
                    using var tcp = new System.Net.Sockets.TcpClient();
                    await tcp.ConnectAsync(host, port);

                    using var stream = tcp.GetStream();
                    await stream.WriteAsync(preamble, 0, preamble.Length);

                    var buffer = new byte[8];
                    var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                    if (bytesRead > 0)
                    {
                        Console.WriteLine("AMQP handshake succeeded.");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Waiting for AMQP handshake... ({ex.Message})");
                }

                await Task.Delay(delayMs);
            }

            throw new TimeoutException($"AMQP handshake failed on {host}:{port} after {timeoutMs}ms");
        }

        public void Dispose()
        {
            try { channel?.Close(); } catch { }
            try { connection?.Close(); } catch { }

            GC.SuppressFinalize(this);
        }
    }
}
