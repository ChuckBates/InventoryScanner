using InventoryScanner.Core.Handlers;
using InventoryScanner.Core.Messages;
using InventoryScanner.Core.Publishers;
using InventoryScanner.Core.Settings;
using InventoryScanner.Messaging.Interfaces;
using InventoryScanner.Messaging.Models;
using RabbitMQ.Client;

namespace InventoryScanner.Core.Observers
{
    public class FetchInventoryMetadataObserver : IRabbitMqSubscriberLifecycleObserver
    {
        private readonly IFetchInventoryMetadataMessageHandler handler;
        private readonly IFetchInventoryMetadataRequestDeadLetterPublisher deadLetterPublisher;
        private readonly RabbitMqSettings rabbitMqSettings;

        public FetchInventoryMetadataObserver(IFetchInventoryMetadataMessageHandler handler, IFetchInventoryMetadataRequestDeadLetterPublisher deadLetterPublisher, IRabbitMqSettings rabbitMqSettings)
        {
            this.handler = handler;
            this.deadLetterPublisher = deadLetterPublisher;
            this.rabbitMqSettings = (RabbitMqSettings) rabbitMqSettings;
        }

        public void OnMessageDeserializationFailed(string queueName, string json, Exception e)
        {
            Console.WriteLine($"Message deserialization failed: {json} - {e.Message}");
        }

        public void OnShutdown(string queueName, ShutdownEventArgs reason)
        {
            Console.WriteLine($"Channel shutdown: {reason.ReplyText}");
        }

        public void OnSubscribed(string queueName)
        {
            Console.WriteLine($"Subscribed to queue {queueName}");
        }

        public void OnSubscriptionFailed(string queueName, Exception e)
        {
            Console.WriteLine($"Subscription to queue {queueName} failed: " + e.Message);
        }

        public void OnUnsubscribed(string queueName, string reason)
        {
            Console.WriteLine($"Subscription {queueName} unsubscribed : {reason}");
        }

        public void OnMessageReceived(string queueName, IRabbitMqMessage message)
        {
            if (message is FetchInventoryMetadataMessage request)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        Console.WriteLine($"Attempting to handle message for barcode: {request.Barcode}");
                        await handler.Handle(request);
                        Console.WriteLine("Handled");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error handling message, publishing to DLQ: {ex.Message}");
                        await deadLetterPublisher.PublishRequest(request);
                    }
                });
            }
        }
    }
}
