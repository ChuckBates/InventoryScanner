using InventoryScannerCore.Events;
using InventoryScannerCore.Settings;
using RabbitMQ.Client;
using Silverback.Messaging;
using Silverback.Messaging.Configuration;
using Silverback.Messaging.Configuration.Rabbit;

namespace InventoryScannerCore
{
    public class RabbitEndpointsConfigurator : IEndpointsConfigurator
    {
        ISettingsService settings;

        public RabbitEndpointsConfigurator(ISettingsService settings)
        {
            this.settings = settings;    
        }

        public void Configure(IEndpointsConfigurationBuilder builder)
        {
            var rabbitSettings = settings.GetRabbitMqSettings();
            builder
                .AddOutbound<FetchInventoryMetadataEvent>(
                    new RabbitExchangeProducerEndpoint("fetch-inventory-metadata")
                    {
                        Connection = new RabbitConnectionConfig
                        {
                            HostName = rabbitSettings.HostName,
                            UserName = rabbitSettings.UserName,
                            Password = rabbitSettings.Password
                        },
                        Exchange = new RabbitExchangeConfig
                        {
                            IsDurable = true,
                            IsAutoDeleteEnabled = false,
                            ExchangeType = ExchangeType.Fanout,
                        }
                    }
                )
                .AddInbound(
                    new RabbitExchangeConsumerEndpoint("fetch-inventory-metadata")
                    {
                        Connection = new RabbitConnectionConfig
                        {
                            HostName = rabbitSettings.HostName,
                            UserName = rabbitSettings.UserName,
                            Password = rabbitSettings.Password
                        },
                        Exchange = new RabbitExchangeConfig
                        {
                            IsDurable = true,
                            IsAutoDeleteEnabled = false,
                            ExchangeType = ExchangeType.Fanout
                        },
                        QueueName = "fetch-inventory-metadata-queue",
                        Queue = new RabbitQueueConfig
                        {
                            IsDurable = true,
                            IsExclusive = false,
                            IsAutoDeleteEnabled = false
                        }
                    }
                );
        }
    }
}
