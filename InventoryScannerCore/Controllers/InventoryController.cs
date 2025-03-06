using Microsoft.AspNetCore.Mvc;

namespace InventoryScannerCore.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InventoryController : Controller
    {
        [HttpGet(Name = "GetAll")]
        public IEnumerable<Inventory> GetAll()
        {
            return [new Inventory()];
        }
    }
}
