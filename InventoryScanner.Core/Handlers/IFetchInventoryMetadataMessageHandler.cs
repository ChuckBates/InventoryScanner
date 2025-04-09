using InventoryScanner.Core.Messages;

namespace InventoryScanner.Core.Handlers
{
    public interface IFetchInventoryMetadataMessageHandler
    {
        Task Handle(FetchInventoryMetadataMessage message);
    }
}