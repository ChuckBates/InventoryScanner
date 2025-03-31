using InventoryScannerCore.Models;

namespace InventoryScannerCore.Workflows
{
    public class InventoryWorkflowResponse(string status, Inventory data, List<string> errors)
    {
        public string Status { get; set; } = status;
        public Inventory Data { get; set; } = data;
        public List<string> Errors { get; set; } = errors;

        public override bool Equals(object? obj)
        {
            return obj is InventoryWorkflowResponse response &&
                   Status == response.Status &&
                   EqualityComparer<Inventory>.Default.Equals(Data, response.Data) &&
                   Errors.SequenceEqual(response.Errors);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Status, Data, Errors);
        }
    }
}
