using InventoryScanner.Messaging.Interfaces;
using InventoryScanner.Messaging.Models;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace InventoryScanner.Messaging.Subscribing
{
    public class RabbitMqSubscriber : IRabbitMqSubscriber
    {
        private readonly IRabbitMqSettings settings;
        private readonly IRabbitMqConnectionManager connectionManager;
        private readonly IRabbitMqSubscriberLifecycleObserver? lifecycleObserver;
        private readonly ILogger<RabbitMqSubscriber> logger;

        public RabbitMqSubscriber(IRabbitMqConnectionManager connectionManager, IRabbitMqSettings settings, IRabbitMqSubscriberLifecycleObserver? lifecycleObserver, ILogger<RabbitMqSubscriber> logger)
        {
            this.connectionManager = connectionManager;
            this.settings = settings;
            this.lifecycleObserver = lifecycleObserver ?? new EmptyRabbitMqLifecycleObserver();
            this.logger = logger;
        }

        public async Task SubscribeAsync<TMessage>(string queueName, CancellationToken cancellationToken = default) where TMessage : class, IRabbitMqMessage
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
                lifecycleObserver?.OnSubscriptionFailed(queueName, e);
                return;
            }

            channel.ModelShutdown += (sender, args) =>
            {
                lifecycleObserver?.OnShutdown(queueName, args);
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
                        lifecycleObserver?.OnMessageReceived(queueName, message);
                        channel.BasicAck(messageArgs.DeliveryTag, false);
                    }
                    else
                    {
                        channel.BasicNack(messageArgs.DeliveryTag, false, false);
                    }
                }
                catch (Exception e)
                {
                    lifecycleObserver?.OnMessageDeserializationFailed(queueName, json, e);
                    channel.BasicNack(messageArgs.DeliveryTag, false, false);
                }
            };

            var consumerTag = string.Empty;

            try
            {
                consumerTag = channel.BasicConsume(queueName, false, consumer);
                lifecycleObserver?.OnSubscribed(queueName);
            }
            catch (Exception e)
            {
                lifecycleObserver?.OnSubscriptionFailed(queueName, e);
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
                    logger.LogError(ex, "Error occurred while cancelling RabbitMQ consumer.");
                }
            });
        }
    }
}
