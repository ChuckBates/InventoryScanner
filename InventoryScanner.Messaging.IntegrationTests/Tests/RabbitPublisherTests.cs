using InventoryScanner.Messaging.Enums;
using InventoryScanner.Messaging.IntegrationTests.Constructs;
using InventoryScanner.Messaging.IntegrationTests.Helpers;
using InventoryScanner.Messaging.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryScanner.Messaging.IntegrationTests.Tests
{
    [TestFixture]
    [NonParallelizable]
    public class RabbitPublisherTests
    {
        IRabbitMqSettings settings;
        IRabbitMqPublisher publisher;
        string exchangeName;
        string queueName;

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            var testHelper = new IntegrationTestDependencyHelper();
            await testHelper.SpinUp();

            if (testHelper.Provider == null)
            {
                throw new InvalidOperationException("Provider is not initialized.");
            }

            publisher = testHelper.Provider.GetRequiredService<IRabbitMqPublisher>();
            settings = testHelper.Provider.GetRequiredService<IRabbitMqSettings>();
            exchangeName = settings.ExchangeName;
            queueName = settings.QueueName;
        }

        [Test]
        public async Task When_publishing_a_message()
        {
            var barcode = "1234567890";
            var testMessage = new TestMessage
            {
                Barcode = barcode,
                MessageId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow
            };

            using (var rabbit = new RabbitTestHelper(settings))
            {
                await publisher.PublishAsync(testMessage, exchangeName);

                var messages = await rabbit.ReadMessages<TestMessage>(queueName, 1);

                Assert.That(messages, Is.Not.Null);
                Assert.That(messages.Count, Is.EqualTo(1));
                Assert.That(messages.First().Barcode, Is.EqualTo(barcode));
                Assert.That(messages.First().MessageId, Is.TypeOf<Guid>());
                Assert.That((DateTime.UtcNow - messages.First().Timestamp).TotalSeconds, Is.LessThan(5));

                await rabbit.PurgeQueue(queueName);
            };
        }

        [Test]
        public async Task When_publishing_multiple_messages()
        {
            var barcode1 = "1234567890";
            var testMessage1 = new TestMessage
            {
                Barcode = barcode1,
                MessageId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow
            };
            var barcode2 = "1234567892";
            var testMessage2 = new TestMessage
            {
                Barcode = barcode2,
                MessageId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow
            };
            var barcode3 = "1234567893";
            var testMessage3 = new TestMessage
            {
                Barcode = barcode3,
                MessageId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow
            };

            using (var rabbit = new RabbitTestHelper(settings))
            {
                await publisher.PublishAsync(testMessage1, exchangeName);
                await publisher.PublishAsync(testMessage2, exchangeName);
                await publisher.PublishAsync(testMessage3, exchangeName);

                var messages = await rabbit.ReadMessages<TestMessage>(queueName, 3);

                Assert.That(messages, Is.Not.Null);
                Assert.That(messages.Count, Is.EqualTo(3));

                Assert.That(messages[0].Barcode, Is.EqualTo(barcode1));
                Assert.That(messages[1].Barcode, Is.EqualTo(barcode2));
                Assert.That(messages[2].Barcode, Is.EqualTo(barcode3));

                await rabbit.PurgeQueue(queueName);
            };
        }

        [Test]
        public async Task When_publishing_a_message_and_rabbit_is_unreachable()
        {
            var barcode = "45689155685";
            var testMessage = new TestMessage
            {
                Barcode = barcode,
                MessageId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow
            };

            using (var rabbit = new RabbitTestHelper(settings))
            {
                await rabbit.ShutdownRabbitMqAsync();
                var result = await publisher.PublishAsync(testMessage, exchangeName);

                Assert.That(result.Status, Is.EqualTo(PublisherResponseStatus.Failure));
                Assert.That(result.Data.Count, Is.EqualTo(1));
                Assert.That(result.Data.First(), Is.TypeOf<TestMessage>());
                Assert.That(((TestMessage)result.Data.First()).Barcode, Is.EqualTo(barcode));
                Assert.That(result.Errors.Count, Is.EqualTo(1));
                Assert.That(result.Errors.First().StartsWith("RabbitMQ Error: Unable to reach rabbit host. Message: "));

                await rabbit.StartRabbitMqAsync();
                await rabbit.PurgeQueue(queueName, true);
            }
        }

        [Test]
        public async Task When_publishing_a_message_and_the_publish_fails_transiently()
        {
            var barcode = "45689155685";
            var testMessage = new TestMessage
            {
                Barcode = barcode,
                MessageId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow
            };

            using (var rabbit = new RabbitTestHelper(settings))
            {
                await rabbit.RestartRabbitMqAsync();

                var result = await publisher.PublishAsync(testMessage, exchangeName);

                Assert.That(result.Status, Is.EqualTo(PublisherResponseStatus.Success));
                Assert.That(result.Data.Count, Is.EqualTo(1));
                Assert.That(result.Data.First(), Is.TypeOf<TestMessage>());
                Assert.That(((TestMessage)result.Data.First()).Barcode, Is.EqualTo(barcode));
                Assert.That(result.Errors.Count, Is.EqualTo(0));

                await rabbit.PurgeQueue(queueName, true);
            }
        }
    }
}