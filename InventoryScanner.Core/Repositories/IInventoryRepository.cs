using InventoryScanner.Core.Models;

namespace InventoryScanner.Core.Repositories
{
    public interface IInventoryRepository
    {
        Task Delete(string barcode);
        Task<IEnumerable<Inventory>> GetAll();
        Task<Inventory?> Get(string barcode);
        Task<int> Insert(Inventory inventory);
    }
}