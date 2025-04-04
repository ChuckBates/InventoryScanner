
using EasyNetQ;
using InventoryScanner.Messaging.Models;

namespace InventoryScannerCore.Events
{
    [Queue("fetch-inventory-metadata-queue", ExchangeName = "fetch-inventory-metadata-exchange")]
    public class FetchInventoryMetadataEvent : IRabbitMqEvent
    {
        public required string Barcode { get; set; }
        public Guid EventId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
