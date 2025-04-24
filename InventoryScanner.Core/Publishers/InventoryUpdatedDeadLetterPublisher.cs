using InventoryScanner.Core.Messages;
using InventoryScanner.Core.Publishers.Interfaces;
using InventoryScanner.Core.Settings;
using InventoryScanner.Messaging.Interfaces;
using InventoryScanner.Messaging.Publishing;

namespace InventoryScanner.Core.Publishers
{
    public class InventoryUpdatedDeadLetterPublisher : RabbitMqPublisherBase, IInventoryUpdatedDeadLetterPublisher
    {
        private readonly RabbitMqSettings settings;

        public InventoryUpdatedDeadLetterPublisher(IRabbitMqPublisher publisher, ISettingsService settings) : base(publisher)
        {
            this.settings = settings.GetRabbitMqSettings();
        }

        public async Task<PublisherResponse> Publish(InventoryUpdatedMessage message)
        {
            return await PublishAsync(message, settings.InventoryUpdatedDeadLetterExchangeName);
        }
    }
}
