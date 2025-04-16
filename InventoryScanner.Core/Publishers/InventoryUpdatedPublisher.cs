using InventoryScanner.Core.Messages;
using InventoryScanner.Core.Models;
using InventoryScanner.Core.Publishers.Interfaces;
using InventoryScanner.Core.Settings;
using InventoryScanner.Logging;
using InventoryScanner.Messaging.Interfaces;
using InventoryScanner.Messaging.Publishing;

namespace InventoryScanner.Core.Publishers
{
    public class InventoryUpdatedPublisher : RabbitMqPublisherBase, IInventoryUpdatedPublisher
    {
        private readonly RabbitMqSettings settings;
        private readonly IAppLogger<InventoryUpdatedPublisher> logger;

        public InventoryUpdatedPublisher(IRabbitMqPublisher publisher, ISettingsService settings, IAppLogger<InventoryUpdatedPublisher> logger) : base(publisher)
        {
            this.settings = settings.GetRabbitMqSettings();
            this.logger = logger;
        }

        public async Task<PublisherResponse> Publish(Inventory updatedInventory)
        {
            var message = new InventoryUpdatedMessage
            {
                Barcode = updatedInventory.Barcode,
                UpdatedInventory = updatedInventory,
                MessageId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow
            };

            logger.Info(new LogContext
            {
                Barcode = updatedInventory.Barcode,
                Component = typeof(InventoryUpdatedPublisher).Name,
                Message = "Publishing InventoryUpdatedMessage.",
                Operation = "Publish"
            });
            return await PublishAsync(message, settings.InventoryUpdatedExchangeName);
        }
    }
}
