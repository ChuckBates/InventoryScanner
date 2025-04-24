using InventoryScanner.Core.Enums;

namespace InventoryScanner.Core.Workflows
{

    public class ImageWorkflowResponse(string status, byte[] data, List<string> errors)
    {
        public string Status { get; set; } = status;
        public byte[] Data { get; set; } = data;
        public List<string> Errors { get; set; } = errors;
        public static ImageWorkflowResponse Failure(string message) =>
            new(WorkflowResponseStatus.Failure, [], [message]);

        public static ImageWorkflowResponse Success(byte[] data) =>
            new(WorkflowResponseStatus.Success, data, []);

        public override bool Equals(object? obj)
        {
            return obj is ImageWorkflowResponse response &&
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
