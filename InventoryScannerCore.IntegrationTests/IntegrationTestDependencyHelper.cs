using InventoryScannerCore.Controllers;
using InventoryScannerCore.Publishers;
using InventoryScannerCore.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using EasyNetQ;
using System.Diagnostics;
using System.Net.Sockets;
using RabbitMQ.Client;

namespace InventoryScannerCore.IntegrationTests
{
    public class IntegrationTestDependencyHelper
    {
        public IServiceProvider provider { get; private set; }
        public IConnection rabbitConnection { get; set; }
        public IModel rabbitChannel { get; set; }

        private ServiceCollection services = new();

        public async Task SpinUp(bool withRabbit)
        {
            AddSettingsService();

            if (withRabbit)
            {
                AddRabbitServices();
            }

            provider = services.BuildServiceProvider();

            if (withRabbit)
            {
                await StartRabbitAsync();
            }

            await StartServicesAsync();
        }

        private async Task StartServicesAsync()
        {
            var hostedServices = provider.GetServices<IHostedService>();
            foreach (var hostedService in hostedServices)
            {
                await hostedService.StartAsync(CancellationToken.None);
            }
        }

        private void AddSettingsService()
        {
            services.AddLogging(builder => builder.AddConsole());

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json");

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                config.AddUserSecrets<InventoryController>();
            }

            var configBuilder = config.Build();
            services.Configure<Settings.Settings>(configBuilder.GetSection("Settings"));
            services.Configure<RabbitMqSettings>(configBuilder.GetSection("RabbitMQ"));

            services.AddScoped<ISettingsService, SettingsService>();
        }

        private void AddRabbitServices()
        {
            var settingsService = services.BuildServiceProvider().GetRequiredService<ISettingsService>();
            var rabbitSettings = settingsService.GetRabbitMqSettings();
            var connectionString = $"host={rabbitSettings.HostName}:{rabbitSettings.AmqpPort};username={rabbitSettings.UserName};password={rabbitSettings.Password}";
            services.AddSingleton(RabbitHutch.CreateBus(connectionString));

            services.AddScoped<IFetchInventoryMetadataRequestPublisher, FetchInventoryMetadataRequestPublisher>();
            services.AddSingleton<IHostApplicationLifetime, FakeHostApplicationLifetime>();
        }

        private async Task StartRabbitAsync()
        {
            var settingsService = provider.GetRequiredService<ISettingsService>();
            var rabbitMqSettings = settingsService.GetRabbitMqSettings();

            var factory = new ConnectionFactory
            {
                HostName = rabbitMqSettings.HostName,
                UserName = rabbitMqSettings.UserName,
                Password = rabbitMqSettings.Password,
                Port = rabbitMqSettings.AmqpPort
            };

            await WaitForPortOpen(rabbitMqSettings.HostName, rabbitMqSettings.ManagementPort, timeoutSeconds: 10);
            //await WaitForRabbitMqManagementApiAsync(rabbitMqSettings.HostName);

            rabbitConnection = factory.CreateConnection();
            rabbitChannel = rabbitConnection.CreateModel();
            rabbitChannel.ExchangeDeclare(
                exchange: rabbitMqSettings.FetchInventoryMetadataExchangeName,
                type: ExchangeType.Fanout,
                durable: true,
                autoDelete: false,
                arguments: null);

            rabbitChannel.QueueDeclare(
                queue: rabbitMqSettings.FetchInventoryMetadataQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            rabbitChannel.QueueBind(
                queue: rabbitMqSettings.FetchInventoryMetadataQueueName,
                exchange: rabbitMqSettings.FetchInventoryMetadataExchangeName,
                routingKey: "");
        }

        private static async Task WaitForPortOpen(string host, int port, int timeoutSeconds)
        {
            var timeout = TimeSpan.FromSeconds(timeoutSeconds);
            var stopwatch = Stopwatch.StartNew();

            while (stopwatch.Elapsed < timeout)
            {
                try
                {
                    using var client = new TcpClient();
                    var connectTask = client.ConnectAsync(host, port);
                    var completed = await Task.WhenAny(connectTask, Task.Delay(500));
                    if (completed == connectTask && client.Connected)
                    {
                        return;
                    }
                }
                catch
                {
                    // ignore and retry
                }

                await Task.Delay(200);
            }

            throw new TimeoutException($"Timed out waiting for {host}:{port} to become available.");
        }
    }

    internal class FakeHostApplicationLifetime : IHostApplicationLifetime
    {
        public CancellationToken ApplicationStarted => CancellationToken.None;
        public CancellationToken ApplicationStopping => CancellationToken.None;
        public CancellationToken ApplicationStopped => CancellationToken.None;

        public void StopApplication() { }
    }
}
