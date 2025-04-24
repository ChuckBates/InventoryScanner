using TestMessage = InventoryScanner.Messaging.IntegrationTests.Constructs.TestMessage;
using InventoryScanner.Messaging.IntegrationTests.Helpers;
using InventoryScanner.Messaging.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client.Exceptions;
using System.Text.Json;
using InventoryScanner.Messaging.Publishing;

namespace InventoryScanner.Messaging.IntegrationTests.Tests
{
    [TestFixture]
    [NonParallelizable]
    public class RabbitSubscriberTests
    {
        IRabbitMqSettings settings;
        IRabbitMqPublisher publisher;
        IRabbitMqSubscriber subscriber;
        TestSubscriberLifecycleObserver lifecycleObserver;
        string exchangeName;
        string queueName;

        [OneTimeSetUp]
        public async Task OneTimeSetUpAsync()
        {
            var testHelper = new IntegrationTestDependencyHelper();
            await testHelper.SpinUp();

            if (testHelper.Provider == null)
            {
                throw new InvalidOperationException("Provider is not initialized.");
            }

            settings = testHelper.Provider.GetRequiredService<IRabbitMqSettings>();
            publisher = testHelper.Provider.GetRequiredService<IRabbitMqPublisher>();
            subscriber = testHelper.Provider.GetRequiredService<IRabbitMqSubscriber>();
            lifecycleObserver = testHelper.Provider.GetRequiredService<TestSubscriberLifecycleObserver>();
            exchangeName = settings.ExchangeName;
            queueName = settings.QueueName;
        }

        [SetUp]
        public void Setup()
        {
            lifecycleObserver.Clear();
        }

        [Test]
        public async Task When_subscribing_to_a_queue_and_one_message_is_received()
        {
            var barcode = "1234984156";
            var testMessage = new TestMessage
            {
                Barcode = barcode,
                MessageId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow
            };

            var subscriberCancellationTokenSource = new CancellationTokenSource();

            await subscriber.SubscribeAsync<TestMessage>(queueName, lifecycleObserver, subscriberCancellationTokenSource.Token);
            await Task.Delay(100);

            await publisher.PublishAsync(testMessage, exchangeName);
            await Task.Delay(1000);

            Assert.That(lifecycleObserver.IsSubscribed, Is.True);
            Assert.That(lifecycleObserver.IsMessageReceived, Is.True);
            Assert.That(lifecycleObserver.ReceivedMessages, Is.Not.Empty);
            Assert.That(((TestMessage)lifecycleObserver.ReceivedMessages.First()).Barcode, Is.EqualTo(testMessage.Barcode));
        }

        [Test]
        public async Task When_subscribing_to_a_queue_and_multiple_messages_are_received()
        {
            var barcode1 = "1234567890";
            var barcode2 = "1234567892";
            var barcode3 = "1234567893";

            var message1 = new TestMessage { Barcode = barcode1, MessageId = Guid.NewGuid(), Timestamp = DateTime.UtcNow };
            var message2 = new TestMessage { Barcode = barcode2, MessageId = Guid.NewGuid(), Timestamp = DateTime.UtcNow };
            var message3 = new TestMessage { Barcode = barcode3, MessageId = Guid.NewGuid(), Timestamp = DateTime.UtcNow };

            var subscriberCancellationTokenSource = new CancellationTokenSource();

            await subscriber.SubscribeAsync<TestMessage>(queueName, lifecycleObserver, subscriberCancellationTokenSource.Token);
            await Task.Delay(100);

            await publisher.PublishAsync(message1, exchangeName);
            await publisher.PublishAsync(message2, exchangeName);
            await publisher.PublishAsync(message3, exchangeName);
            await Task.Delay(1000);

            Assert.That(lifecycleObserver.IsMessageReceived, Is.True);
            Assert.That(lifecycleObserver.ReceivedMessages, Is.Not.Empty);
            Assert.That(lifecycleObserver.ReceivedMessages.Count, Is.EqualTo(3));

            Assert.That(((TestMessage) lifecycleObserver.ReceivedMessages[0]).Barcode, Is.EqualTo(barcode1));
            Assert.That(((TestMessage) lifecycleObserver.ReceivedMessages[1]).Barcode, Is.EqualTo(barcode2));
            Assert.That(((TestMessage) lifecycleObserver.ReceivedMessages[2]).Barcode, Is.EqualTo(barcode3));
        }

        [Test]
        public async Task When_subscribing_to_a_queue_and_rabbit_is_unreachable()
        {
            lifecycleObserver.Clear();

            using (var rabbitHelper = new RabbitTestHelper(settings))
            {
                await rabbitHelper.ShutdownRabbitMqAsync();

                var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                await subscriber.SubscribeAsync<TestMessage>(queueName, lifecycleObserver, cancellationTokenSource.Token);

                Assert.That(lifecycleObserver.IsSubscriptionFailed, Is.True);
                Assert.That(lifecycleObserver.SubscriptionFailedException, Is.Not.Null);
                Assert.That(lifecycleObserver.SubscriptionFailedException, Is.TypeOf<BrokerUnreachableException>());

                cancellationTokenSource.Cancel();

                await rabbitHelper.StartRabbitMqAsync();
            }
        }

        [Test]
        public async Task When_subscribing_to_a_queue_and_the_subscribe_fails_transiently_while_already_comsuming()
        {
            var testMessage = new TestMessage
            {
                Barcode = "1234567891",
                MessageId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow
            };
            var cancelTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            using (var rabbitHelper = new RabbitTestHelper(settings))
            {
                await subscriber.SubscribeAsync<TestMessage>(queueName, lifecycleObserver, cancelTokenSource.Token);

                await rabbitHelper.ShutdownRabbitMqAsync();
                await Task.Delay(1000);

                Assert.That(lifecycleObserver.IsShutdown, Is.True);
                lifecycleObserver.Clear();

                await rabbitHelper.StartRabbitMqAsync();
                await Task.Delay(1000);

                await publisher.PublishAsync(testMessage, exchangeName);
                await Task.Delay(500);

                await subscriber.SubscribeAsync<TestMessage>(queueName, lifecycleObserver, cancelTokenSource.Token);
                await Task.Delay(500);

                Assert.That(lifecycleObserver.IsSubscribed, Is.True);
                Assert.That(lifecycleObserver.IsMessageReceived, Is.True);
                Assert.That(lifecycleObserver.ReceivedMessages, Is.Not.Empty);
                Assert.That(((TestMessage) lifecycleObserver.ReceivedMessages.First()).Barcode, Is.EqualTo(testMessage.Barcode));
            }
        }

        [Test]
        public async Task When_subscribing_to_a_queue_and_the_subscribe_fails_transiently_while_attempting_to_comsume()
        {
            var testMessage = new TestMessage
            {
                Barcode = "1234567891",
                MessageId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow
            };
            var cancelTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            using (var rabbitHelper = new RabbitTestHelper(settings))
            {
                await rabbitHelper.ShutdownRabbitMqAsync();
                await Task.Delay(1000);

                var subscribeTask = subscriber.SubscribeAsync<TestMessage>(queueName, lifecycleObserver, cancelTokenSource.Token);
                await Task.Delay(1000);

                Assert.That(lifecycleObserver.IsSubscriptionFailed, Is.False);

                await rabbitHelper.StartRabbitMqAsync();
                await Task.Delay(1000);

                await subscribeTask;
                Assert.That(lifecycleObserver.IsSubscribed, Is.True);

                await publisher.PublishAsync(testMessage, exchangeName);
                await Task.Delay(500);

                Assert.That(lifecycleObserver.IsMessageReceived, Is.True);
                Assert.That(lifecycleObserver.ReceivedMessages, Is.Not.Empty);
                Assert.That(((TestMessage) lifecycleObserver.ReceivedMessages.First()).Barcode, Is.EqualTo(testMessage.Barcode));
            }
        }

        [Test]
        public async Task When_subscribing_to_a_queue_and_the_message_fails_to_deserialize()
        {
            var badJson = "{ \"Barcode\": 123 }";

            var cancelTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            await subscriber.SubscribeAsync<TestMessage>(queueName, lifecycleObserver, cancelTokenSource.Token);
            await Task.Delay(100);

            var rawPublisher = publisher as RabbitMqPublisher;
            await rawPublisher.RawPublishAsync(badJson, exchangeName);
            await Task.Delay(1000);

            Assert.That(lifecycleObserver.IsMessageDeserializationFailed, Is.True);
            Assert.That(lifecycleObserver.IsMessageReceived, Is.False);
            Assert.That(lifecycleObserver.MessageDeserializationFailedJson, Is.EqualTo(JsonSerializer.Serialize(badJson)));
            Assert.That(lifecycleObserver.MessageDeserializationFailedException, Is.TypeOf<JsonException>());
        }
    }
}
