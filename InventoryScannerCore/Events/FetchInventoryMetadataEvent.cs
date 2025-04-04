
using EasyNetQ;

namespace InventoryScannerCore.Events
{
    [Queue("fetch-inventory-metadata-queue", ExchangeName = "fetch-inventory-metadata-exchange")]
    public class FetchInventoryMetadataEvent : IRabbitEvent
    {
        public required string Barcode { get; set; }
        public string EventId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
