using InventoryScanner.Core.Enums;
using InventoryScanner.Core.Models;

namespace InventoryScanner.Core.Workflows
{
    public class InventoryWorkflowResponse(string status, List<Inventory> data, List<string> errors)
    {
        public string Status { get; set; } = status;
        public List<Inventory> Data { get; set; } = data;
        public List<string> Errors { get; set; } = errors;
        public static InventoryWorkflowResponse Failure(string message) =>
            new(WorkflowResponseStatus.Failure, [], [message]);

        public static InventoryWorkflowResponse Success(List<Inventory> items) =>
            new(WorkflowResponseStatus.Success, items, []);

        public override bool Equals(object? obj)
        {
            return obj is InventoryWorkflowResponse response &&
                   Status == response.Status &&
                   Data.SequenceEqual(response.Data) &&
                   Errors.SequenceEqual(response.Errors);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Status, Data, Errors);
        }
    }
}
