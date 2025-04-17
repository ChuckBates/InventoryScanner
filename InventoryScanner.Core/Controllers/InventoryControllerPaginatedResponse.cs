using InventoryScanner.Core.Models;

namespace InventoryScanner.Core.Controllers
{
    public class InventoryControllerPaginatedResponse(string status, List<Inventory> data, string error = "", int page = 1, int pageSize = 50, bool hasMore = false)
    {
        public string Status { get; set; } = status;
        public List<Inventory> Data { get; set; } = data;
        public string Error { get; set; } = error;
        public int Page { get; set; } = page;
        public int PageSize { get; set; } = pageSize;
        public bool HasMore { get; set; } = hasMore;
    }
}
