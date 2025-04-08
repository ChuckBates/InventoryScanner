using InventoryScanner.Messaging.Interfaces;
using InventoryScanner.Messaging.Models;
using RabbitMQ.Client;

namespace InventoryScanner.Messaging.Subscribing
{
    public class EmptyRabbitMqLifecycleObserver : IRabbitMqSubscriberLifecycleObserver
    {
        public void OnMessageDeserializationFailed(string queueName, string json, Exception e) {}

        public void OnMessageReceived(string queueName, IRabbitMqMessage message) {}

        public void OnShutdown(string queueName, ShutdownEventArgs reason) {}

        public void OnSubscribed(string queueName) {}

        public void OnSubscriptionFailed(string queueName, Exception e) {}

        public void OnUnsubscribed(string queueName, string reason) {}
    }
}
