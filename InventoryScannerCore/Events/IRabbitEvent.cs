namespace InventoryScannerCore.Events
{
    public interface IRabbitEvent
    {
        string EventId { get; }
        DateTime Timestamp { get; }
    }
}
