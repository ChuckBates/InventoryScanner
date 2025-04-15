using InventoryScanner.Core.Handlers;
using InventoryScanner.Core.Messages;
using InventoryScanner.Core.Publishers.Interfaces;
using InventoryScanner.Core.Settings;
using InventoryScanner.Core.Subscribers;
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
        private readonly ILogger<FetchInventoryMetadataObserver> logger;
        private string subscriberName = typeof(FetchInventoryMetadataSubscriber).Name;

        public FetchInventoryMetadataObserver(
            IFetchInventoryMetadataMessageHandler handler, 
            IFetchInventoryMetadataRequestDeadLetterPublisher deadLetterPublisher, 
            IRabbitMqSettings rabbitMqSettings, 
            ILogger<FetchInventoryMetadataObserver> logger)
        {
            this.handler = handler;
            this.deadLetterPublisher = deadLetterPublisher;
            this.rabbitMqSettings = (RabbitMqSettings)rabbitMqSettings;
            this.logger = logger;
        }

        public void OnMessageDeserializationFailed(string queueName, string json, Exception e)
        {
            logger.LogError(e, "{Name}: message deserialization failed: {json}", subscriberName, json);
        }

        public void OnShutdown(string queueName, ShutdownEventArgs reason)
        {
            logger.LogError(reason.Exception, "{Name}: channel shutdown: {reason}", subscriberName, reason.ReplyText);
        }

        public void OnSubscribed(string queueName)
        {
            logger.LogInformation("{Name}: subscribed to queue {queueName}", subscriberName, queueName);
        }

        public void OnSubscriptionFailed(string queueName, Exception e)
        {
            logger.LogError(e, "{Name}: subscription to queue {queueName} failed", subscriberName, queueName);
        }

        public void OnUnsubscribed(string queueName, string reason)
        {
            logger.LogWarning("{Name}: subscription {queueName} unsubscribed: {reason}", subscriberName, queueName, reason);
        }

        public void OnMessageReceived(string queueName, IRabbitMqMessage message)
        {
            if (message is FetchInventoryMetadataMessage request)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        logger.LogInformation("{Name}: attempting to handle {queue} message for barcode: {barcode}", subscriberName, queueName, request.Barcode);
                        await handler.Handle(request);
                        logger.LogInformation("{Name}: handled message for barcode: {barcode}", subscriberName, request.Barcode);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "{Name}: error handling message for barcode: {barcode}, publishing to DLQ", subscriberName, request.Barcode);
                        await deadLetterPublisher.PublishRequest(request);
                    }
                });
            }
        }
    }
}
