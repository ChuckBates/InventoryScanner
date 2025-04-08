using InventoryScanner.Messaging.Models;

namespace InventoryScanner.Messaging.Interfaces
{
    public interface IRabbitMqSubscriber
    {
        Task SubscribeAsync<TMessage>(string queueName, CancellationToken cancellationToken) where TMessage : class, IRabbitMqMessage;
    }
}
