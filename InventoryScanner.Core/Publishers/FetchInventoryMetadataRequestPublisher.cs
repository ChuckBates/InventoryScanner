using InventoryScanner.Messaging.Interfaces;
using InventoryScanner.Core.Settings;
using InventoryScanner.Messaging.Publishing;
using InventoryScanner.Core.Messages;
using InventoryScanner.Core.Publishers.Interfaces;
using InventoryScanner.Logging;

namespace InventoryScanner.Core.Publishers
{
    public class FetchInventoryMetadataRequestPublisher : RabbitMqPublisherBase, IFetchInventoryMetadataRequestPublisher
    {
        private readonly RabbitMqSettings settings;
        private readonly IAppLogger<FetchInventoryMetadataRequestPublisher> logger;

        public FetchInventoryMetadataRequestPublisher(IRabbitMqPublisher publisher, ISettingsService settings, IAppLogger<FetchInventoryMetadataRequestPublisher> logger) : base(publisher)
        {
            this.settings = settings.GetRabbitMqSettings();
            this.logger = logger;
        }

        public async Task<PublisherResponse> Publish(string barcode)
        {
            var message = new FetchInventoryMetadataMessage
            {
                Barcode = barcode,
                MessageId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow
            };

            logger.Info(new LogContext
            {
                Barcode = barcode,
                Component = typeof(FetchInventoryMetadataRequestPublisher).Name,
                Message = "Publishing FetchInventoryMetadataMessage.",
                Operation = "Publish Request"
            });
            return await PublishAsync(message, settings.FetchInventoryMetadataExchangeName);
        }
    }
}
