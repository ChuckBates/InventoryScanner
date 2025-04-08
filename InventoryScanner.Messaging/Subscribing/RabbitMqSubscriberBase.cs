using InventoryScanner.Messaging.Interfaces;
using InventoryScanner.Messaging.Models;

namespace InventoryScanner.Messaging.Subscribing
{
    public abstract class RabbitMqSubscriberBase
    {
        private readonly IRabbitMqSubscriber rabbitMqSubscriber;

        protected RabbitMqSubscriberBase(IRabbitMqSubscriber rabbitMqSubscriber)
        {
            this.rabbitMqSubscriber = rabbitMqSubscriber;
        }

        protected Task SubscribeAsync<T>(string exchangeName, CancellationToken cancellationToken) where T : class, IRabbitMqMessage
        {
            return rabbitMqSubscriber.SubscribeAsync<T>(exchangeName, cancellationToken);
        }
    }
}
