using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using InventoryScanner.Messaging.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using InventoryScanner.Messaging.IntegrationTests.Constructs;
using InventoryScanner.Messaging.Infrastructure;
using InventoryScanner.Logging;

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

            services.Configure<List<RabbitMqInfrastructureTarget>>(opts =>
            {
                opts.Add(new RabbitMqInfrastructureTarget
                {
                    ExchangeName = rabbitSettings.ExchangeName,
                    QueueName = rabbitSettings.QueueName,
                    ExchangeType = "fanout"
                });
            });
            services.AddMessaging(connectionString, startup: false);
            services.AddSingleton<TestSubscriberLifecycleObserver>();
            services.AddSingleton(typeof(IAppLogger<>), typeof(AppLogger<>));
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
        }
    }
}
