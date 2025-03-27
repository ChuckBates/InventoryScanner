using InventoryScannerCore.Models;

namespace InventoryScannerCore.Repositories
{
    public interface IInventoryRepository
    {
        IEnumerable<Inventory> GetAll();
        Inventory? Get(string barcode);
        void Insert(Inventory inventory);
    }
}