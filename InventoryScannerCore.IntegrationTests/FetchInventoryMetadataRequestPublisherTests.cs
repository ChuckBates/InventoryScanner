using InventoryScanner.Messaging.Enums;
using InventoryScannerCore.Events;
using InventoryScannerCore.Publishers;
using InventoryScannerCore.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryScannerCore.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public class FetchInventoryMetadataRequestPublisherTests
    {
        [Test]
        public async Task When_publishing_a_fetch_inventory_metadata_event()
        {
            var testHelper = new IntegrationTestDependencyHelper();
            await testHelper.SpinUp(withRabbit: true);

            if (testHelper.Provider == null)
            {
                throw new InvalidOperationException("Provider is not initialized.");
            }

            var publisher = testHelper.Provider.GetRequiredService<IFetchInventoryMetadataRequestPublisher>();
            var settings = testHelper.Provider.GetRequiredService<ISettingsService>();
            var queueName = settings.GetRabbitMqSettings().FetchInventoryMetadataQueueName;

            using (var rabbit = new RabbitTestContext(settings.GetRabbitMqSettings()))
            {
                var barcode = "1234567890";

                await publisher.RequestFetchInventoryMetadata(barcode);
                var messages = await rabbit.ReadMessages<FetchInventoryMetadataEvent>(queueName, 1);

                Assert.That(messages, Is.Not.Null);
                Assert.That(messages.Count, Is.EqualTo(1));
                Assert.That(messages.First().Barcode, Is.EqualTo(barcode));
                Assert.That(messages.First().EventId, Is.TypeOf<Guid>());
                Assert.That((DateTime.UtcNow - messages.First().Timestamp).TotalSeconds, Is.LessThan(5));

                await rabbit.PurgeQueue(queueName);
            };
        }

        [Test]
        public async Task When_publishing_multiple_fetch_inventory_metadata_events()
        {
            var testHelper = new IntegrationTestDependencyHelper();
            await testHelper.SpinUp(withRabbit: true);

            if (testHelper.Provider == null)
            {
                throw new InvalidOperationException("Provider is not initialized.");
            }

            var publisher = testHelper.Provider.GetRequiredService<IFetchInventoryMetadataRequestPublisher>();
            var settings = testHelper.Provider.GetRequiredService<ISettingsService>();
            var queueName = settings.GetRabbitMqSettings().FetchInventoryMetadataQueueName;

            using (var rabbit = new RabbitTestContext(settings.GetRabbitMqSettings()))
            {
                var barcode1 = "1234567890";
                var barcode2 = "1234567892";
                var barcode3 = "1234567893";

                await publisher.RequestFetchInventoryMetadata(barcode1);
                await publisher.RequestFetchInventoryMetadata(barcode2);
                await publisher.RequestFetchInventoryMetadata(barcode3);

                var messages = await rabbit.ReadMessages<FetchInventoryMetadataEvent>(queueName, 3);

                Assert.That(messages, Is.Not.Null);
                Assert.That(messages.Count, Is.EqualTo(3));

                Assert.That(messages[0].Barcode, Is.EqualTo(barcode1));
                Assert.That(messages[1].Barcode, Is.EqualTo(barcode2));
                Assert.That(messages[2].Barcode, Is.EqualTo(barcode3));

                await rabbit.PurgeQueue(queueName);
            };
        }

        [Test]
        public async Task When_publishing_a_fetch_inventory_metadata_event_and_rabbit_is_unreachable()
        {
            var testHelper = new IntegrationTestDependencyHelper();
            await testHelper.SpinUp(withRabbit: true);

            if (testHelper.Provider == null)
            {
                throw new InvalidOperationException("Provider is not initialized.");
            }

            var publisher = testHelper.Provider.GetRequiredService<IFetchInventoryMetadataRequestPublisher>();
            var rabbitChannel = testHelper.RabbitChannel;
            var rabbitConnection = testHelper.RabbitConnection;
            var settings = testHelper.Provider.GetRequiredService<ISettingsService>();
            var queueName = settings.GetRabbitMqSettings().FetchInventoryMetadataQueueName;

            using (var rabbit = new RabbitTestContext(settings.GetRabbitMqSettings()))
            {
                var barcode = "45689155685";

                await rabbit.ShutdownRabbitMqAsync();
                var result = await publisher.RequestFetchInventoryMetadata(barcode);

                Assert.That(result.Status, Is.EqualTo(PublisherResponseStatus.Failure));
                Assert.That(result.Data.Count, Is.EqualTo(1));
                Assert.That(result.Data.First(), Is.TypeOf<FetchInventoryMetadataEvent>());
                Assert.That(((FetchInventoryMetadataEvent)result.Data.First()).Barcode, Is.EqualTo(barcode));
                Assert.That(result.Errors.Count, Is.EqualTo(1));
                Assert.That(result.Errors.First().StartsWith("RabbitMQ Error: Unable to reach rabbit host. Message: "));

                await rabbit.StartRabbitMqAsync();
                await rabbit.PurgeQueue(queueName, true);
            }
        }

        [Test]
        public async Task When_publishing_a_fetch_inventory_metadata_event_and_the_publish_fails_transiently()
        {
            var testHelper = new IntegrationTestDependencyHelper();
            await testHelper.SpinUp(withRabbit: true);

            if (testHelper.Provider == null)
            {
                throw new InvalidOperationException("Provider is not initialized.");
            }

            var publisher = testHelper.Provider.GetRequiredService<IFetchInventoryMetadataRequestPublisher>();
            var rabbitChannel = testHelper.RabbitChannel;
            var rabbitConnection = testHelper.RabbitConnection;
            var settings = testHelper.Provider.GetRequiredService<ISettingsService>();
            var queueName = settings.GetRabbitMqSettings().FetchInventoryMetadataQueueName;

            using (var rabbit = new RabbitTestContext(settings.GetRabbitMqSettings()))
            {
                var barcode = "45689155685";

                await rabbit.RestartRabbitMqAsync();

                var result = await publisher.RequestFetchInventoryMetadata(barcode);

                Assert.That(result.Status, Is.EqualTo(PublisherResponseStatus.Success));
                Assert.That(result.Data.Count, Is.EqualTo(1));
                Assert.That(result.Data.First(), Is.TypeOf<FetchInventoryMetadataEvent>());
                Assert.That(((FetchInventoryMetadataEvent)result.Data.First()).Barcode, Is.EqualTo(barcode));
                Assert.That(result.Errors.Count, Is.EqualTo(0));

                await rabbit.PurgeQueue(queueName, true);
            }
        }
    }
}
