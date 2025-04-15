using InventoryScanner.Messaging.Interfaces;
using InventoryScanner.Core.Settings;
using InventoryScanner.Messaging.Publishing;
using InventoryScanner.Core.Messages;
using InventoryScanner.Core.Publishers.Interfaces;

namespace InventoryScanner.Core.Publishers
{
    public class FetchInventoryMetadataRequestPublisher : RabbitMqPublisherBase, IFetchInventoryMetadataRequestPublisher
    {
        private readonly RabbitMqSettings settings;
        private readonly ILogger<FetchInventoryMetadataRequestPublisher> logger;

        public FetchInventoryMetadataRequestPublisher(IRabbitMqPublisher publisher, ISettingsService settings, ILogger<FetchInventoryMetadataRequestPublisher> logger) : base(publisher)
        {
            this.settings = settings.GetRabbitMqSettings();
            this.logger = logger;
        }

        public async Task<PublisherResponse> PublishRequest(string barcode)
        {
            var message = new FetchInventoryMetadataMessage
            {
                Barcode = barcode,
                MessageId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow
            };

            logger.LogInformation("Publishing FetchInventoryMetadataMessage with Barcode: {Barcode}", barcode);
            return await PublishAsync(message, settings.FetchInventoryMetadataExchangeName);
        }
    }
}
