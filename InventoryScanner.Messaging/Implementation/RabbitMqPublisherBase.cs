using InventoryScanner.Messaging.Interfaces;
using InventoryScanner.Messaging.Models;

namespace InventoryScanner.Messaging.Implementation
{
    public abstract class RabbitMqPublisherBase
    {
        private readonly IRabbitMqPublisher rabbitMqPublisher;

        protected RabbitMqPublisherBase(IRabbitMqPublisher rabbitMqPublisher)
        {
            this.rabbitMqPublisher = rabbitMqPublisher;
        }

        protected Task<PublisherResponse> PublishAsync<T>(T message) where T : class, IRabbitMqEvent
        {
            return rabbitMqPublisher.PublishAsync(message);
        }
    }
}
