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

        public FetchInventoryMetadataRequestPublisher(IRabbitMqPublisher publisher, ISettingsService settings) : base(publisher)
        {
            this.settings = settings.GetRabbitMqSettings();
        }

        public async Task<PublisherResponse> PublishRequest(string barcode)
        {
            var message = new FetchInventoryMetadataMessage
            {
                Barcode = barcode,
                MessageId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow
            };

            return await PublishAsync(message, settings.FetchInventoryMetadataExchangeName);
        }
    }
}
