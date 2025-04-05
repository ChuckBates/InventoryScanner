using InventoryScanner.Messaging.Models;
using InventoryScanner.Messaging.Publishing;

namespace InventoryScanner.Messaging.Interfaces
{
    public interface IRabbitMqPublisher
    {
        Task<PublisherResponse> PublishAsync<TEvent>(TEvent message, string exchangeName) where TEvent : class, IRabbitMqEvent;
    }
}
