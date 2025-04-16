using InventoryScanner.Logging;
using InventoryScanner.Messaging.Interfaces;
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
        private readonly IAppLogger<RabbitMqConnectionManager> logger;

        public RabbitMqConnectionManager(IRabbitMqSettings settings, IAppLogger<RabbitMqConnectionManager> logger)
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
                               logger.Error(ex, new LogContext
                               {
                                   Barcode = null,
                                   Component = typeof(RabbitMqConnectionManager).Name,
                                   Message = $"Error occurred while creating RabbitMQ connection. Retrying in {ts.TotalSeconds} seconds...",
                                   Operation = "Get Connection"
                               });
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
