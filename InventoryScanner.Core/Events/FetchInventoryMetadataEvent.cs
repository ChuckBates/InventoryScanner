
using EasyNetQ;
using InventoryScanner.Messaging.Models;

namespace InventoryScanner.Core.Events
{
    [Queue("fetch-inventory-metadata-queue", ExchangeName = "fetch-inventory-metadata-exchange")]
    public class FetchInventoryMetadataEvent : IRabbitMqMessage
    {
        public required string Barcode { get; set; }
        public Guid MessageId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
