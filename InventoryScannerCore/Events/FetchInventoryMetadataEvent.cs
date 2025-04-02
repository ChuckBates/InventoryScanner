
namespace InventoryScannerCore.Events
{
    public class FetchInventoryMetadataEvent : IRabbitEvent
    {
        public required string Barcode { get; set; }
        public string EventId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
