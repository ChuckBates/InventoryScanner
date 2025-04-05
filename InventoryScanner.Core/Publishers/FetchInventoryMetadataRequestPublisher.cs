using InventoryScanner.Messaging.Interfaces;
using InventoryScanner.Core.Events;
using InventoryScanner.Core.Settings;
using InventoryScanner.Messaging.Publishing;

namespace InventoryScanner.Core.Publishers
{
    public class FetchInventoryMetadataRequestPublisher : RabbitMqPublisherBase, IFetchInventoryMetadataRequestPublisher
    {
        private readonly IRabbitMqSettings settings;

        public FetchInventoryMetadataRequestPublisher(IRabbitMqPublisher publisher, ISettingsService settings) : base(publisher)
        {
            this.settings = settings.GetRabbitMqSettings();
        }

        public async Task<PublisherResponse> RequestFetchInventoryMetadata(string barcode)
        {
            var message = new FetchInventoryMetadataEvent
            {
                Barcode = barcode,
                EventId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow
            };

            return await PublishAsync(message, settings.ExchangeName);
        }
    }
}
