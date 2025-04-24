using InventoryScanner.Logging;
using InventoryScanner.Messaging.Interfaces;
using InventoryScanner.Messaging.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace InventoryScanner.Messaging.Subscribing
{
    public class RabbitMqSubscriber : IRabbitMqSubscriber
    {
        private readonly IRabbitMqSettings settings;
        private readonly IRabbitMqConnectionManager connectionManager;
        private readonly IAppLogger<RabbitMqSubscriber> logger;

        public RabbitMqSubscriber(IRabbitMqConnectionManager connectionManager, IRabbitMqSettings settings, IAppLogger<RabbitMqSubscriber> logger)
        {
            this.connectionManager = connectionManager;
            this.settings = settings;
            this.logger = logger;
        }

        public async Task SubscribeAsync<TMessage>(string queueName, IRabbitMqSubscriberLifecycleObserver observer, CancellationToken cancellationToken) where TMessage : class, IRabbitMqMessage
        {
            IConnection connection;
            IModel channel;
            try
            {
                connection = await connectionManager.GetConnectionAsync();
                channel = connection.CreateModel();
            }
            catch (Exception e)
            {
                observer.OnSubscriptionFailed(queueName, e);
                return;
            }

            channel.ModelShutdown += (sender, args) =>
            {
                observer.OnShutdown(queueName, args);
            };

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.Received += async (model, messageArgs) =>
            {
                var body = messageArgs.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);

                try
                {
                    var message = System.Text.Json.JsonSerializer.Deserialize<TMessage>(json);

                    if (message != null)
                    {
                        observer.OnMessageReceived(queueName, message);
                        channel.BasicAck(messageArgs.DeliveryTag, false);
                    }
                    else
                    {
                        channel.BasicNack(messageArgs.DeliveryTag, false, false);
                    }
                }
                catch (Exception e)
                {
                    observer.OnMessageDeserializationFailed(queueName, json, e);
                    channel.BasicNack(messageArgs.DeliveryTag, false, false);
                }
            };

            var consumerTag = string.Empty;

            try
            {
                consumerTag = channel.BasicConsume(queueName, false, consumer);
                observer.OnSubscribed(queueName);
            }
            catch (Exception e)
            {
                observer.OnSubscriptionFailed(queueName, e);
                return;
            }

            cancellationToken.Register(() =>
            {
                try
                {
                    channel.BasicCancel(consumerTag);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, new LogContext
                    {
                        Barcode = null,
                        Component = typeof(RabbitMqSubscriber).Name,
                        Message = "Error occurred while cancelling RabbitMQ consumer.",
                        Operation = "Subscribe"
                    });
                }
            });
        }
    }
}
