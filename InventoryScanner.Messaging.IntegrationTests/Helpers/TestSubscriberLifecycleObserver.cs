using InventoryScanner.Messaging.Interfaces;
using InventoryScanner.Messaging.Models;
using RabbitMQ.Client;

namespace InventoryScanner.Messaging.IntegrationTests.Helpers
{
    public class TestSubscriberLifecycleObserver : IRabbitMqSubscriberLifecycleObserver
    {
        public bool IsMessageDeserializationFailed { get; private set; }
        public bool IsMessageReceived { get; private set; }
        public bool IsShutdown { get; private set; }
        public bool IsSubscribed { get; private set; }
        public bool IsSubscriptionFailed { get; private set; }
        public bool IsUnsubscribed { get; private set; }

        public string? QueueName { get; private set; }
        public string? MessageDeserializationFailedJson { get; private set; }
        public Exception? MessageDeserializationFailedException { get; private set; }
        public IList<IRabbitMqMessage> ReceivedMessages { get; private set; } = [];
        public ShutdownEventArgs? ShutdownReason { get; private set; }
        public Exception? SubscriptionFailedException { get; private set; }
        public string? UnsubscribedReason { get; private set; }

        public void OnMessageDeserializationFailed(string queueName, string json, Exception e)
        {
            IsMessageDeserializationFailed = true;

            QueueName = queueName;
            MessageDeserializationFailedJson = json;
            MessageDeserializationFailedException = e;
        }

        public void OnMessageReceived(string queueName, IRabbitMqMessage message)
        {
            IsMessageReceived = true;

            QueueName = queueName;
            ReceivedMessages.Add(message);
        }

        public void OnShutdown(string queueName, ShutdownEventArgs reason)
        {
            IsShutdown = true;

            QueueName = queueName;
            ShutdownReason = reason;
        }

        public void OnSubscribed(string queueName)
        {
            IsSubscribed = true;

            QueueName = queueName;
        }

        public void OnSubscriptionFailed(string queueName, Exception e)
        {
            IsSubscriptionFailed = true;

            QueueName = queueName;
            SubscriptionFailedException = e;
        }

        public void OnUnsubscribed(string queueName, string reason)
        {
            IsUnsubscribed = true;

            QueueName = queueName;
            UnsubscribedReason = reason;
        }

        public void Clear()
        {
            IsMessageDeserializationFailed = false;
            IsMessageReceived = false;
            IsShutdown = false;
            IsSubscribed = false;
            IsSubscriptionFailed = false;
            IsUnsubscribed = false;
            QueueName = null;
            MessageDeserializationFailedJson = null;
            MessageDeserializationFailedException = null;
            ReceivedMessages = [];
            ShutdownReason = null;
            SubscriptionFailedException = null;
            UnsubscribedReason = null;
        }
    }
}
