using InventoryScanner.Messaging.Implementation;

namespace InventoryScanner.Core.Publishers
{
    public interface IFetchInventoryMetadataRequestPublisher
    {
        Task<PublisherResponse> RequestFetchInventoryMetadata(string barcode);
    }
}
