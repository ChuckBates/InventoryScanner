using InventoryScanner.Core.Models;

namespace InventoryScanner.Core.Repositories
{
    public interface IInventoryRepository
    {
        Task Delete(string barcode);
        Task<Inventory?> Get(string barcode);
        Task<int> Insert(Inventory inventory);
        Task<IEnumerable<Inventory>> GetAll(DateTime since, int pageNumber = 1, int pageSize = 50);
    }
}