using InventoryScanner.Core.Models;
using InventoryScanner.Messaging.Models;

namespace InventoryScanner.Core.Messages
{
    public class InventoryUpdatedMessage : IRabbitMqMessage
    {
        public required string Barcode { get; set; }
        public required Guid MessageId { get; set; }
        public required DateTime Timestamp { get; set; }
        public required Inventory UpdatedInventory { get; set; }
    }
}
