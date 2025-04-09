using InventoryScanner.Core.Messages;
using InventoryScanner.Messaging.Interfaces;
using InventoryScanner.Messaging.Models;
using RabbitMQ.Client;
using System.Text.Json;

namespace InventoryScanner.Core.Observers
{
    public class FetchInventoryMetadataObserver : IRabbitMqSubscriberLifecycleObserver
    {
        public void OnMessageDeserializationFailed(string queueName, string json, Exception e)
        {
            Console.WriteLine($"Message deserialization failed: {json} - {e.Message}");
        }

        public void OnMessageReceived(string queueName, IRabbitMqMessage message)
        {
            Console.WriteLine($"Message received: {JsonSerializer.Serialize((FetchInventoryMetadataMessage)message)}");
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
    }
}
