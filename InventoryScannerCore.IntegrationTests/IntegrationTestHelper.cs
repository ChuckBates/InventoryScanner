using InventoryScannerCore.Controllers;
using InventoryScannerCore.Publishers;
using InventoryScannerCore.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace InventoryScannerCore.IntegrationTests
{
    public class IntegrationTestHelper
    {
        public IServiceProvider provider { get; private set; }
        public IConnection rabbitConnection { get; set; }
        public IModel rabbitChannel { get; set; }

        private ServiceCollection services = new ServiceCollection();

        public IntegrationTestHelper(bool withRabbit = false)
        {
            provider = SpinUpWithSettings().Result;

            if (withRabbit)
            {
                provider = SpinUpRabbit().Result;
            }
        }

        public async Task<IServiceProvider> SpinUpWithSettings()
        {
            services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole());

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json");

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                config.AddUserSecrets<InventoryController>();
            }

            var configBuilder = config.Build();
            services.Configure<Settings.Settings>(configBuilder.GetSection("Settings"));

            services.AddScoped<ISettingsService, SettingsService>();

            var provider = services.BuildServiceProvider();

            var hostedServices = provider.GetServices<IHostedService>();
            foreach (var hostedService in hostedServices)
            {
                await hostedService.StartAsync(CancellationToken.None);
            }

            return provider;
        }

        public async Task<IServiceProvider> SpinUpRabbit()
        {
            string fetchInventoryMetadataQueueName = "fetch-inventory-metadata-queue";

            await SpinUpWithSettings();

            services
                .AddSilverback()
                .WithConnectionToMessageBroker(options => options.AddRabbit())
                .AddEndpointsConfigurator<RabbitEndpointsConfigurator>();

            services.AddScoped<ISettingsService, SettingsService>();
            services.AddScoped<IFetchInventoryMetadataRequestPublisher, FetchInventoryMetadataRequestPublisher>();
            services.AddSingleton<IHostApplicationLifetime, FakeHostApplicationLifetime>();

            var provider = services.BuildServiceProvider();
            var settingsService = provider.GetRequiredService<ISettingsService>();
            var rabbitMqSettings = settingsService.GetRabbitMqSettings();

            var factory = new ConnectionFactory
            {
                HostName = rabbitMqSettings.HostName,
                UserName = rabbitMqSettings.UserName,
                Password = rabbitMqSettings.Password,
                Port = rabbitMqSettings.AmqpPort
            };

            rabbitConnection = factory.CreateConnection();
            rabbitChannel = rabbitConnection.CreateModel();
            rabbitChannel.ExchangeDeclare(
                exchange: "fetch-inventory-metadata",
                type: ExchangeType.Fanout,
                durable: true,
                autoDelete: false,
                arguments: null);

            rabbitChannel.QueueDeclare(
                queue: fetchInventoryMetadataQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            rabbitChannel.QueueBind(
                queue: fetchInventoryMetadataQueueName,
                exchange: "fetch-inventory-metadata",
                routingKey: "");

            var hostedServices = provider.GetServices<IHostedService>();
            foreach (var hostedService in hostedServices)
            {
                await hostedService.StartAsync(CancellationToken.None);
            }

            return provider;
        }

        public async Task<IServiceProvider> SpinUpBlackHoleRabbit(bool startHost = false)
        {
            await SpinUpWithSettings();

            services
                .AddSilverback()
                .WithConnectionToMessageBroker(options => options.AddRabbit())
                .AddEndpointsConfigurator<RabbitEndpointsConfigurator>();

            services.AddScoped<ISettingsService, SettingsService>();
            services.AddScoped<IFetchInventoryMetadataRequestPublisher, FetchInventoryMetadataRequestPublisher>();
            services.AddSingleton<IHostApplicationLifetime, FakeHostApplicationLifetime>();

            services.Configure<Settings.Settings>(opts =>
            {
                opts.RabbitMQ = new RabbitMqSettings
                {
                    HostName = "unreachable-host",
                    UserName = "guest",
                    Password = "guest",
                    AmqpPort = 5672,
                    FetchInventoryMetadataExchangeName = "fetch-inventory-metadata-exchange-test" + Guid.NewGuid(),
                };

                // Provide dummy values for required fields
                opts.DatabaseServer = "localhost";
                opts.DatabasePort = 5432;
                opts.DatabaseName = "test";
                opts.DatabaseUser = "user";
                opts.DatabasePassword = "pass";
                opts.RapidApiKey = "test-key";
                opts.RapidApiHost = "test-host";
            });

            var provider = services.BuildServiceProvider();

            if (startHost)
            {
                var hostedServices = provider.GetServices<IHostedService>();
                foreach (var hostedService in hostedServices)
                {
                    await hostedService.StartAsync(CancellationToken.None);
                }
            }

            return provider;
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
