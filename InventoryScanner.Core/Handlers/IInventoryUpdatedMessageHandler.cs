using InventoryScanner.Core.Messages;

namespace InventoryScanner.Core.Handlers
{
    public interface IInventoryUpdatedMessageHandler
    {
        Task Handle(InventoryUpdatedMessage message);
    }
}