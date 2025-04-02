using InventoryScannerCore.Enums;
using InventoryScannerCore.Events;
using RabbitMQ.Client.Exceptions;
using Silverback.Messaging.Broker;
using Silverback.Messaging.Publishing;

namespace InventoryScannerCore.Publishers
{
    public class FetchInventoryMetadataRequestPublisher : IFetchInventoryMetadataRequestPublisher
    {
        private readonly IPublisher publisher;

        public FetchInventoryMetadataRequestPublisher(IPublisher publisher)
        {
            this.publisher = publisher;
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
                await publisher.PublishAsync(message);
            }
            catch (ProduceException e)
            {
                response.Status = PublisherResponseStatus.Error;
                response.Errors.Add("RabbitMQ Error: Unable to reach rabbit host. Message: " + e.Message);
            }

            return response;
        }
    }
}
