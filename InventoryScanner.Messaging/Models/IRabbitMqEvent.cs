namespace InventoryScanner.Messaging.Models
{
    public interface IRabbitMqEvent
    {
        Guid EventId { get; }
        DateTime Timestamp { get; }
    }
}
