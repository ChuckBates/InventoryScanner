using InventoryScannerCore.Models;

namespace InventoryScannerCore.Workflows
{
    public interface IInventoryWorkflow
    {
        Task<InventoryWorkflowResponse> Add(Inventory inventory);
        Task<InventoryWorkflowResponse> Get(string barcode);
        Task<InventoryWorkflowResponse> GetAll();
    }
}