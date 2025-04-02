using InventoryScannerCore.Events;

namespace InventoryScannerCore.Publishers
{
    public class PublisherResponse(string status, List<IRabbitEvent> data, List<string> errors)
    {
        public string Status { get; set; } = status;
        public List<IRabbitEvent> Data { get; set; } = data;
        public List<string> Errors { get; set; } = errors;
    }
}
