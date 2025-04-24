using InventoryScanner.Messaging.Models;

namespace InventoryScanner.Messaging.Interfaces
{
    public interface IRabbitMqSubscriber
    {
        Task SubscribeAsync<TMessage>(string queueName, IRabbitMqSubscriberLifecycleObserver observer, CancellationToken cancellationToken) where TMessage : class, IRabbitMqMessage;
    }
}
