using InventoryScanner.Messaging.Interfaces;
using InventoryScanner.Messaging.Models;

namespace InventoryScanner.Messaging.Publishing
{
    public abstract class RabbitMqPublisherBase
    {
        private readonly IRabbitMqPublisher rabbitMqPublisher;

        protected RabbitMqPublisherBase(IRabbitMqPublisher rabbitMqPublisher)
        {
            this.rabbitMqPublisher = rabbitMqPublisher;
        }

        protected Task<PublisherResponse> PublishAsync<T>(T message, string exchangeName) where T : class, IRabbitMqMessage
        {
            return rabbitMqPublisher.PublishAsync(message, exchangeName);
        }
    }
}
