using InventoryScannerCore.Models;
using InventoryScannerCore.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace InventoryScannerCore.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InventoryController(IInventoryRepository inventoryRepository) : Controller
    {
        [HttpGet(Name = "GetAll")]
        public IEnumerable<Inventory> GetAll()
        {
            return inventoryRepository.GetAll();
        }
    }
}
