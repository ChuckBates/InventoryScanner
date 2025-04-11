using InventoryScanner.Messaging.Interfaces;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;

namespace InventoryScanner.Messaging.Infrastructure
{
    public class RabbitMqConnectionManager : IRabbitMqConnectionManager
    {
        private readonly ConnectionFactory connectionFactory;
        private readonly IRabbitMqSettings settings;
        private IConnection? connection;
        private readonly SemaphoreSlim semaphore = new(1, 1);
        private readonly ILogger<RabbitMqConnectionManager> logger;

        public RabbitMqConnectionManager(IRabbitMqSettings settings, ILogger<RabbitMqConnectionManager> logger)
        {
            this.settings = settings;
            connectionFactory = new ConnectionFactory
            {
                HostName = settings.HostName,
                Port = settings.AmqpPort,
                UserName = settings.UserName,
                Password = settings.Password,
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true
            };
            this.logger = logger;
        }

        public async Task<IConnection> GetConnectionAsync()
        {
            await semaphore.WaitAsync();

            try
            {
                if (connection == null || !connection.IsOpen)
                {
                    var retryPolicy = Policy
                       .Handle<Exception>()
                       .WaitAndRetryAsync(
                           retryCount: settings.SubscribeRetryCount,
                           sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                           onRetry: (ex, ts) =>
                           {
                               logger.LogError(ex, "Error occurred while creating RabbitMQ connection. Retrying in {TimeSpan} seconds...", ts.TotalSeconds);
                           });

                    connection?.Dispose();
                    connection = await retryPolicy.ExecuteAsync(() => Task.FromResult(connectionFactory.CreateConnection()));
                }

                return connection;
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
