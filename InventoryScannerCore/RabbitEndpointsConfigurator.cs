using InventoryScannerCore.Events;
using InventoryScannerCore.Settings;
using RabbitMQ.Client;
using Silverback.Messaging;
using Silverback.Messaging.Broker;
using Silverback.Messaging.Configuration;
using Silverback.Messaging.Configuration.Rabbit;
using Silverback.Messaging.Messages;
using Silverback.Messaging.Outbound;

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
                    new RabbitExchangeProducerEndpoint(rabbitSettings.FetchInventoryMetadataExchangeName)
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
                            Arguments = new Dictionary<string, object>
                            {
                               
                            }
                        },
                        ConfirmationTimeout = TimeSpan.FromSeconds(10)
                    }
                )
                .AddInbound(
                    new RabbitExchangeConsumerEndpoint(rabbitSettings.FetchInventoryMetadataExchangeName)
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
                        QueueName = rabbitSettings.FetchInventoryMetadataQueueName,
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
