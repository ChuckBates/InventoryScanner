using InventoryScanner.Messaging.Enums;

namespace InventoryScanner.Messaging.Models
{
    public interface IPublisherResponse
    {
        PublisherResponseStatus Status { get; }
        IReadOnlyList<IRabbitMqMessage> Data { get; }
        IReadOnlyList<string> Errors { get; }
    }
}
