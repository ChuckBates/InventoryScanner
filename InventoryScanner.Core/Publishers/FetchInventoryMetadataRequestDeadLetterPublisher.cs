using InventoryScanner.Core.Messages;
using InventoryScanner.Core.Publishers.Interfaces;
using InventoryScanner.Core.Settings;
using InventoryScanner.Messaging.Interfaces;
using InventoryScanner.Messaging.Publishing;

namespace InventoryScanner.Core.Publishers
{
    public class FetchInventoryMetadataRequestDeadLetterPublisher : RabbitMqPublisherBase, IFetchInventoryMetadataRequestDeadLetterPublisher
    {
        private readonly RabbitMqSettings settings;

        public FetchInventoryMetadataRequestDeadLetterPublisher(IRabbitMqPublisher publisher, ISettingsService settings) : base(publisher)
        {
            this.settings = settings.GetRabbitMqSettings();
        }

        public async Task<PublisherResponse> PublishRequest(FetchInventoryMetadataMessage message)
        {
            return await PublishAsync(message, settings.FetchInventoryMetadataDeadLetterExchangeName);
        }
    }
}
