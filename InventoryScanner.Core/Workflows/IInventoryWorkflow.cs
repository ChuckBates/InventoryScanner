using InventoryScanner.Core.Models;

namespace InventoryScanner.Core.Workflows
{
    public interface IInventoryWorkflow
    {
        Task<InventoryWorkflowResponse> Add(Inventory inventory);
        Task<InventoryWorkflowResponse> Get(string barcode);
        Task<InventoryWorkflowResponse> GetAll();
        Task<InventoryWorkflowResponse> Update(Inventory inventory, bool refetch = false);
    }
}