using InventoryScanner.Messaging.Models;

namespace InventoryScanner.Messaging.IntegrationTests.Constructs
{
    public class TestEvent : IRabbitMqEvent
    {
        public required string Barcode { get; set; }
        public Guid EventId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
