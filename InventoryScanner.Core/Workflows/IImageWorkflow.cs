
namespace InventoryScanner.Core.Workflows
{
    public interface IImageWorkflow
    {
        Task<ImageWorkflowResponse> Get(string imagePath);
    }
}