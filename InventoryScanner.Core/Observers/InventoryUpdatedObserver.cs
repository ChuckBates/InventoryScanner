using InventoryScanner.Core.Handlers;
using InventoryScanner.Core.Messages;
using InventoryScanner.Core.Publishers.Interfaces;
using InventoryScanner.Logging;
using InventoryScanner.Messaging.Interfaces;
using InventoryScanner.Messaging.Models;
using RabbitMQ.Client;

namespace InventoryScanner.Core.Observers
{
    public class InventoryUpdatedObserver : IRabbitMqSubscriberLifecycleObserver
    {
        private readonly IInventoryUpdatedMessageHandler handler;
        private readonly IInventoryUpdatedDeadLetterPublisher deadLetterPublisher;
        private readonly IAppLogger<InventoryUpdatedObserver> logger;
        private string subscriberName = typeof(InventoryUpdatedObserver).Name;

        public InventoryUpdatedObserver(
            IInventoryUpdatedMessageHandler handler,
            IInventoryUpdatedDeadLetterPublisher deadLetterPublisher, 
            IAppLogger<InventoryUpdatedObserver> logger)
        {
            this.handler = handler;
            this.deadLetterPublisher = deadLetterPublisher;
            this.logger = logger;
        }

        public void OnMessageDeserializationFailed(string queueName, string json, Exception e)
        {
            logger.Error(e, new LogContext
            {
                Barcode = null,
                Component = subscriberName,
                Message = $"Message on queue {queueName} deserialization failed: {json}",
                Operation = "Observer On Message Deserialization Failed"
            });
        }

        public void OnShutdown(string queueName, ShutdownEventArgs reason)
        {
            logger.Warning(new LogContext
            {
                Barcode = null,
                Component = subscriberName,
                Message = $"{queueName} channel shutdown: {reason.ReplyText}",
                Operation = "Observer On Shutdown"
            });
        }

        public void OnSubscribed(string queueName)
        {
            logger.Info(new LogContext
            {
                Barcode = null,
                Component = subscriberName,
                Message = $"Subscribed to queue {queueName}",
                Operation = "Observer On Subscribed"
            });
        }

        public void OnSubscriptionFailed(string queueName, Exception e)
        {
            logger.Error(e, new LogContext
            {
                Barcode = null,
                Component = subscriberName,
                Message = $"Subscription to queue {queueName} failed.",
                Operation = "Observer On Subscription Failed"
            });
        }

        public void OnUnsubscribed(string queueName, string reason)
        {
            logger.Warning(new LogContext
            {
                Barcode = null,
                Component = subscriberName,
                Message = $"{queueName} subscription unsubscribed: {reason}",
                Operation = "Observer On UnSubscribed"
            });
        }

        public void OnMessageReceived(string queueName, IRabbitMqMessage message)
        {
            if (message is InventoryUpdatedMessage request)
            {
                _ = Task.Run(async () =>
                {
                    var serializedMessage = System.Text.Json.JsonSerializer.Serialize(request);
                    try
                    {
                        logger.Info(new LogContext
                        {
                            Barcode = request.Barcode,
                            Component = subscriberName,
                            Message = $"Attempting to handle {queueName} message: {serializedMessage}",
                            Operation = "Observer On Message Received"
                        });
                        await handler.Handle(request);
                        logger.Info(new LogContext
                        {
                            Barcode = request.Barcode,
                            Component = subscriberName,
                            Message = $"Handled {queueName} message.",
                            Operation = "Observer On Message Received"
                        });
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, new LogContext
                        {
                            Barcode = null,
                            Component = subscriberName,
                            Message = $"Error handling {queueName} message, publishing to DLQ: {serializedMessage}",
                            Operation = "Observer On Message Received"
                        });
                        await deadLetterPublisher.Publish(request);
                    }
                });
            }
        }
    }
}
