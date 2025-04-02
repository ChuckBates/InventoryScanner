namespace InventoryScannerCore.Publishers
{
    public interface IFetchInventoryMetadataRequestPublisher
    {
        Task RequestFetchInventoryMetadata(string barcode);
    }
}
