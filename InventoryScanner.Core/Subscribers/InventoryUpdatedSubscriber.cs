using InventoryScanner.Core.Messages;
using InventoryScanner.Core.Observers;
using InventoryScanner.Core.Settings;
using InventoryScanner.Logging;
using InventoryScanner.Messaging.Interfaces;
using Polly;

namespace InventoryScanner.Core.Subscribers
{
    public class InventoryUpdatedSubscriber : BackgroundService
    {
        private readonly IRabbitMqSubscriber subscriber;
        private readonly RabbitMqSettings settings;
        private readonly IAppLogger<InventoryUpdatedSubscriber> logger;
        private readonly InventoryUpdatedObserver observer;

        public InventoryUpdatedSubscriber(
            IRabbitMqSubscriber subscriber,
            InventoryUpdatedObserver observer,
            IRabbitMqSettings settings, 
            IAppLogger<InventoryUpdatedSubscriber> logger)
        {
            this.subscriber = subscriber;
            this.observer = observer;
            this.settings = (RabbitMqSettings)settings;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: settings.SubscribeRetryCount,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (ex, ts) =>
                    {
                        logger.Warning(new LogContext
                        {
                            Barcode = null,
                            Component = typeof(InventoryUpdatedSubscriber).Name,
                            Message = $"Error occurred while subscribing to RabbitMQ. Retrying in {ts.TotalSeconds} seconds...",
                            Operation = "Execute"
                        });
                    });

            await retryPolicy.ExecuteAsync(async () => 
                await subscriber.SubscribeAsync<InventoryUpdatedMessage>(
                    queueName: settings.InventoryUpdatedQueueName, 
                    observer: observer, 
                    cancellationToken: stoppingToken
                )
            );
        }
    }
}
