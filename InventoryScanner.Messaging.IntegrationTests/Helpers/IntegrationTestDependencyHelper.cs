using Microsoft.Extensions.DependencyInjection;
using EasyNetQ;
using RabbitMQ.Client;
using InventoryScanner.Messaging.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using InventoryScanner.Messaging.IntegrationTests.Constructs;
using InventoryScanner.Messaging.Publishing;
using InventoryScanner.Messaging.Subscribing;

namespace InventoryScanner.Messaging.IntegrationTests.Helpers
{
    public class IntegrationTestDependencyHelper
    {
        public IServiceProvider? Provider { get; private set; }
        public IConnection? RabbitConnection { get; set; }
        public IModel? RabbitChannel { get; set; }

        private ServiceCollection services = new();

        public async Task SpinUp()
        {
            AddSettingsService();

            await ConnectToRabbitAsync();

            AddRabbitServices();

            Provider = services.BuildServiceProvider();

            await StartHostedServicesAsync();
        }

        private async Task StartHostedServicesAsync()
        {
            if (Provider == null)
            {
                throw new InvalidOperationException("Provider is not initialized.");
            }

            var hostedServices = Provider.GetServices<IHostedService>();
            foreach (var hostedService in hostedServices)
            {
                await hostedService.StartAsync(CancellationToken.None);
            }
        }

        private void AddSettingsService()
        {
            services.AddLogging(builder => builder.AddConsole());

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json", optional: false)
                .Build();

            services.Configure<TestRabbitMqSettings>(config.GetSection("Settings:RabbitMQ"));
            services.AddSingleton<IRabbitMqSettings>(sp => sp.GetRequiredService<IOptions<TestRabbitMqSettings>>().Value);
        }

        private void AddRabbitServices()
        {
            using var tempProvider = services.BuildServiceProvider();
            var rabbitSettings = tempProvider.GetRequiredService<IRabbitMqSettings>();
            var connectionString = $"host={rabbitSettings.HostName}:{rabbitSettings.AmqpPort};username={rabbitSettings.UserName};password={rabbitSettings.Password}";
            services.AddSingleton(RabbitHutch.CreateBus(connectionString));
            services.AddSingleton<IRabbitMqConnectionManager, RabbitMqConnectionManager>();
            services.AddSingleton<IRabbitMqSubscriberLifecycleObserver, TestSubscriberLifecycleObserver>();

            services.AddSingleton<IRabbitMqPublisher>(provider =>
            {
                var settings = provider.GetRequiredService<IRabbitMqSettings>();
                var bus = provider.GetRequiredService<IBus>();
                return new RabbitMqPublisher(settings, bus);
            });
            services.AddSingleton<IRabbitMqSubscriber>(provider =>
            {
                var settings = provider.GetRequiredService<IRabbitMqSettings>();
                var connectionManager = provider.GetRequiredService<IRabbitMqConnectionManager>();
                var subscriberLifecycleObserver = provider.GetRequiredService<IRabbitMqSubscriberLifecycleObserver>();
                return new RabbitMqSubscriber(connectionManager, settings, subscriberLifecycleObserver);
            });
        }

        private async Task ConnectToRabbitAsync()
        {
            using var tempProvider = services.BuildServiceProvider();
            var rabbitMqSettings = tempProvider.GetRequiredService<IRabbitMqSettings>();

            var factory = new ConnectionFactory
            {
                HostName = rabbitMqSettings.HostName,
                UserName = rabbitMqSettings.UserName,
                Password = rabbitMqSettings.Password,
                Port = rabbitMqSettings.AmqpPort,
                DispatchConsumersAsync = true
            };

            await RabbitTestHelper.WaitForRabbitMqManagementApiAsync(rabbitMqSettings.HostName, rabbitMqSettings.ManagementPort, timeoutSeconds: 10);

            RabbitConnection = factory.CreateConnection();
            RabbitChannel = RabbitConnection.CreateModel();

            RabbitChannel.ExchangeDeclare(
                exchange: rabbitMqSettings.ExchangeName,
                type: ExchangeType.Fanout,
                durable: true,
                autoDelete: false,
                arguments: null);

            RabbitChannel.QueueDeclare(
                queue: rabbitMqSettings.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            RabbitChannel.QueueBind(
                queue: rabbitMqSettings.QueueName,
                exchange: rabbitMqSettings.ExchangeName,
                routingKey: "");
        }
    }
}
