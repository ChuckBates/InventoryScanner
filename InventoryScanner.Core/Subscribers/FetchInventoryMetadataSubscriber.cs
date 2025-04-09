using InventoryScanner.Core.Messages;
using InventoryScanner.Core.Settings;
using InventoryScanner.Messaging.Interfaces;
using Polly;

namespace InventoryScanner.Core.Subscribers
{
    public class FetchInventoryMetadataSubscriber : BackgroundService
    {
        private readonly IRabbitMqSubscriber subscriber;
        private readonly RabbitMqSettings settings;

        public FetchInventoryMetadataSubscriber(IRabbitMqSubscriber subscriber, IRabbitMqSettings settings)
        {
            this.subscriber = subscriber;
            this.settings = (RabbitMqSettings)settings;
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
                               Console.WriteLine($"[Retry] {ex.GetType().Name}: {ex.Message}");
                           });

            await retryPolicy.ExecuteAsync(async () => await subscriber.SubscribeAsync<FetchInventoryMetadataMessage>(queueName: settings.FetchInventoryMetadataQueueName, cancellationToken: stoppingToken));
        }
    }
}
