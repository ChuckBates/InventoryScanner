using InventoryScannerCore.Models;

namespace InventoryScannerCore.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        public IEnumerable<Inventory> GetAll()
        {
            return new List<Inventory>();
        }
    }
}
