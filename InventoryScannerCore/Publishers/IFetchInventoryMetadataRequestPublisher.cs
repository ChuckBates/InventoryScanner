using InventoryScanner.Messaging.Implementation;

namespace InventoryScannerCore.Publishers
{
    public interface IFetchInventoryMetadataRequestPublisher
    {
        Task<PublisherResponse> RequestFetchInventoryMetadata(string barcode);
    }
}
