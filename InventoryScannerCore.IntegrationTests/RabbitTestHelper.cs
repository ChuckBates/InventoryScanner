using Docker.DotNet.Models;
using Docker.DotNet;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

namespace InventoryScannerCore.IntegrationTests
{
    public static class RabbitTestHelper
    {
        public static async Task<List<T>> WaitForMessagesAsync<T>(
           IModel channel,
           string queueName,
           int expectedCount,
           int maxAttempts = 30,
           int delayMs = 100)
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

        public static async Task RestartRabbitMqAsync(
            string containerName = "rabbitmq",
            string host = "localhost",
            int amqpPort = 5672,
            int managementPort = 15672,
            string username = "guest",
            string password = "guest",
            int retryDelayMs = 500,
            int timeoutSeconds = 20)
        {
            var docker = new DockerClientConfiguration().CreateClient();

            try
            {
                Console.WriteLine($"Restarting RabbitMQ container '{containerName}'...");
                await docker.Containers.RestartContainerAsync(containerName, new ContainerRestartParameters());
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to restart RabbitMQ container '{containerName}' via Docker.",
                    ex);
            }

            await WaitForRabbitMqManagementApiAsync(host, managementPort, username, password, retryDelayMs, timeoutSeconds);
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
    }
}
