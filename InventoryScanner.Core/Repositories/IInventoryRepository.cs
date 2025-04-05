using InventoryScannerCore.Models;

namespace InventoryScannerCore.Repositories
{
    public interface IInventoryRepository
    {
        Task Delete(string barcode);
        Task<IEnumerable<Inventory>> GetAll();
        Task<Inventory?> Get(string barcode);
        Task<int> Insert(Inventory inventory);
    }
}