using InventoryScanner.Core.Messages;
using InventoryScanner.Messaging.Publishing;

namespace InventoryScanner.Core.Publishers
{
    public interface IFetchInventoryMetadataRequestDeadLetterPublisher
    {
        Task<PublisherResponse> PublishRequest(FetchInventoryMetadataMessage message);
    }
}
