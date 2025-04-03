using InventoryScannerCore.Enums;
using InventoryScannerCore.Events;
using Polly;
using Silverback.Messaging.Broker;
using Silverback.Messaging.Publishing;

namespace InventoryScannerCore.Publishers
{
    public class FetchInventoryMetadataRequestPublisher : IFetchInventoryMetadataRequestPublisher
    {
        private readonly IPublisher publisher;
        private readonly AsyncPolicy retryPolicy;

        public FetchInventoryMetadataRequestPublisher(IPublisher publisher)
        {
            this.publisher = publisher;
            this.retryPolicy = Policy
                .Handle<ProduceException>()
                .WaitAndRetryAsync(
                    retryCount: 5, 
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(2),
                    onRetry: (ex, timespan) =>
                    {
                        Console.WriteLine($"Retrying due to: {ex.Message}");
                    });
        }

        public async Task<PublisherResponse> RequestFetchInventoryMetadata(string barcode)
        {
            var message = new FetchInventoryMetadataEvent
            {
                Barcode = barcode,
                EventId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow
            };
            var response = new PublisherResponse(PublisherResponseStatus.Success, [message], []);

            try
            {
                await retryPolicy.ExecuteAsync(async () => 
                    {
                        Console.WriteLine("Publishing message to RabbitMQ");
                        await publisher.PublishAsync(message);
                    });
            }
            catch (Exception e)
            {
                Console.WriteLine($"RabbitMQ Error: Unable to reach rabbit host. Message: {e.Message}");
                response.Status = PublisherResponseStatus.Error;
                response.Errors.Add("RabbitMQ Error: Unable to reach rabbit host. Message: " + e.Message);
            }

            return response;
        }
    }
}
