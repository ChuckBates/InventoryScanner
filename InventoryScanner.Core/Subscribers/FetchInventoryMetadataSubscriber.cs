using InventoryScanner.Core.Messages;
using InventoryScanner.Core.Observers;
using InventoryScanner.Core.Settings;
using InventoryScanner.Logging;
using InventoryScanner.Messaging.Interfaces;
using Polly;

namespace InventoryScanner.Core.Subscribers
{
    public class FetchInventoryMetadataSubscriber : BackgroundService
    {
        private readonly IRabbitMqSubscriber subscriber;
        private readonly RabbitMqSettings settings;
        private readonly IAppLogger<FetchInventoryMetadataSubscriber> logger;
        private readonly FetchInventoryMetadataObserver observer;

        public FetchInventoryMetadataSubscriber(
            IRabbitMqSubscriber subscriber,
            FetchInventoryMetadataObserver observer,
            IRabbitMqSettings settings, 
            IAppLogger<FetchInventoryMetadataSubscriber> logger)
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
                            Component = typeof(FetchInventoryMetadataSubscriber).Name,
                            Message = $"Error occurred while subscribing to RabbitMQ. Retrying in {ts.TotalSeconds} seconds...",
                            Operation = "Execute"
                        });
                    });

            await retryPolicy.ExecuteAsync(async () => 
                await subscriber.SubscribeAsync<FetchInventoryMetadataMessage>(
                    queueName: settings.FetchInventoryMetadataQueueName, 
                    observer: observer, 
                    cancellationToken: stoppingToken
                )
            );
        }
    }
}
