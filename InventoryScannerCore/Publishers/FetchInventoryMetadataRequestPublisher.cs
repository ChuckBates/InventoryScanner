using InventoryScannerCore.Events;
using Silverback.Messaging.Publishing;

namespace InventoryScannerCore.Publishers
{
    public class FetchInventoryMetadataRequestPublisher : IFetchInventoryMetadataRequestPublisher
    {
        private readonly IPublisher _publisher;

        public FetchInventoryMetadataRequestPublisher(IPublisher publisher)
        {
            _publisher = publisher;
        }

        public Task RequestFetchInventoryMetadata(string barcode)
        {
            var message = new FetchInventoryMetadataEvent
            {
                Barcode = barcode,
                EventId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow
            };

            return _publisher.PublishAsync(message);
        }
    }
}
