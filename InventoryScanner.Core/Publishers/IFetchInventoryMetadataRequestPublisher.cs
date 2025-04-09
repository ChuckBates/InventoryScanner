using InventoryScanner.Messaging.Publishing;

namespace InventoryScanner.Core.Publishers
{
    public interface IFetchInventoryMetadataRequestPublisher
    {
        Task<PublisherResponse> PublishRequest(string barcode);
    }
}
