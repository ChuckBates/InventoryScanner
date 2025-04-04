using InventoryScanner.Messaging.Implementation;
using InventoryScanner.Messaging.Models;

namespace InventoryScanner.Messaging.Interfaces
{
    public interface IRabbitMqPublisher
    {
        Task<PublisherResponse> PublishAsync<TEvent>(TEvent message) where TEvent : class, IRabbitMqEvent;
    }
}
