
using InventoryScanner.Messaging.Models;

namespace InventoryScanner.Core.Events
{
    public class FetchInventoryMetadataMessage : IRabbitMqMessage
    {
        public required string Barcode { get; set; }
        public Guid MessageId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
