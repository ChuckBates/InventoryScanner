using InventoryScanner.Core.Enums;
using InventoryScanner.Core.Models;

namespace InventoryScanner.Core.Controllers
{
    public class InventoryControllerResponse(string status, List<Inventory> data, string error = "")
    {
        public string Status { get; set; } = status;
        public List<Inventory> Data { get; set; } = data;
        public string Error { get; set; } = error;
    }
}
