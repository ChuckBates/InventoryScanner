using InventoryScanner.Messaging.Models;

namespace InventoryScanner.Messaging.IntegrationTests.Constructs
{
    public class TestMessage : IRabbitMqMessage
    {
        public required string Barcode { get; set; } = null!;
        public Guid MessageId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
