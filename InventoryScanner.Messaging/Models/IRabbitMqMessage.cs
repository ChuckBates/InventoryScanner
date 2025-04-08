namespace InventoryScanner.Messaging.Models
{
    public interface IRabbitMqMessage
    {
        Guid MessageId { get; }
        DateTime Timestamp { get; }
    }
}
