using InventoryScanner.Core.Models;
using InventoryScanner.Messaging.Publishing;

namespace InventoryScanner.Core.Publishers.Interfaces
{
    public interface IInventoryUpdatedPublisher
    {
        Task<PublisherResponse> Publish(Inventory updatedInventory);
    }
}