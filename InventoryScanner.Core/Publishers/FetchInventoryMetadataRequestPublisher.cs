using InventoryScanner.Messaging.Implementation;
using InventoryScanner.Messaging.Interfaces;
using InventoryScanner.Core.Events;

namespace InventoryScannerCore.Publishers
namespace InventoryScanner.Core.Publishers
{
    public class FetchInventoryMetadataRequestPublisher : RabbitMqPublisherBase, IFetchInventoryMetadataRequestPublisher
    {
        public FetchInventoryMetadataRequestPublisher(IRabbitMqPublisher publisher) : base(publisher) {}

        public async Task<PublisherResponse> RequestFetchInventoryMetadata(string barcode)
        {
            var message = new FetchInventoryMetadataEvent
            {
                Barcode = barcode,
                EventId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow
            };

            return await PublishAsync(message);
        }
    }
}
