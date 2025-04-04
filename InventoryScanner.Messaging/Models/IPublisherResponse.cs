using InventoryScanner.Messaging.Enums;

namespace InventoryScanner.Messaging.Models
{
    public interface IPublisherResponse
    {
        PublisherResponseStatus Status { get; }
        IReadOnlyList<IRabbitMqEvent> Data { get; }
        IReadOnlyList<string> Errors { get; }
    }
}
