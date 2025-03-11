using InventoryScannerCore.Models;

namespace InventoryScannerCore.Repositories
{
    public interface IInventoryRepository
    {
        IEnumerable<Inventory> GetAll();
    }
}