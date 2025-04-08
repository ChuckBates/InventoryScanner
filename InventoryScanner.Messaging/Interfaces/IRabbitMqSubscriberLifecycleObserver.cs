using InventoryScanner.Messaging.Models;
using RabbitMQ.Client;

namespace InventoryScanner.Messaging.Interfaces
{
    public interface IRabbitMqSubscriberLifecycleObserver
    {
        void OnSubscribed(string queueName);
        void OnShutdown(string queueName, ShutdownEventArgs reason);
        void OnSubscriptionFailed(string queueName, Exception e);
        void OnMessageReceived(string queueName, IRabbitMqMessage message);
        void OnMessageDeserializationFailed(string queueName, string json, Exception e);
        void OnUnsubscribed(string queueName, string reason);
    }
}
