using Docker.DotNet;
using Docker.DotNet.Models;
using InventoryScannerCore.Enums;
using InventoryScannerCore.Events;
using InventoryScannerCore.Publishers;
using InventoryScannerCore.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;

namespace InventoryScannerCore.IntegrationTests
{
    [TestFixture]
    public class FetchInventoryMetadataRequestPublisherTests
    {
        IntegrationTestHelper testHelper;
        IConnection rabbitConnection;
        IModel rabbitChannel;
        IFetchInventoryMetadataRequestPublisher publisher;
        ISettingsService settings;
        string queueName;

        [SetUp]
        public void Setup()
        {
            testHelper = new IntegrationTestHelper(withRabbit: true);
            publisher = testHelper.provider.GetRequiredService<IFetchInventoryMetadataRequestPublisher>();
            rabbitChannel = testHelper.rabbitChannel;
            rabbitConnection = testHelper.rabbitConnection;
            settings = testHelper.provider.GetRequiredService<ISettingsService>();
            queueName = settings.GetRabbitMqSettings().FetchInventoryMetadataQueueName;
        }

        [TearDown]
        public void TearDown()
        {
            rabbitChannel.QueuePurge(queueName);
            rabbitChannel.Close();
            rabbitConnection.Close();
        }

        [Test]
        public async Task When_publishing_a_fetch_inventory_metadata_event()
        {
            rabbitChannel.QueuePurge(queueName);

            var barcode = "1234567890";

            await publisher.RequestFetchInventoryMetadata(barcode);

            var messages = await RabbitTestHelper.WaitForMessagesAsync<FetchInventoryMetadataEvent>(rabbitChannel, queueName, 1);

            Assert.That(messages, Is.Not.Null);
            Assert.That(messages.Count, Is.EqualTo(1));
            Assert.That(messages.First().Barcode, Is.EqualTo(barcode));
            Assert.That(() => Guid.Parse(messages.First().EventId), Throws.Nothing);
            Assert.That((DateTime.UtcNow - messages.First().Timestamp).TotalSeconds, Is.LessThan(5));
        }

        [Test]
        public async Task When_publishing_multiple_fetch_inventory_metadata_events()
        {
            rabbitChannel.QueuePurge(queueName);

            var barcode1 = "1234567890";
            var barcode2 = "1234567892";
            var barcode3 = "1234567893";

            await publisher.RequestFetchInventoryMetadata(barcode1);
            await publisher.RequestFetchInventoryMetadata(barcode2);
            await publisher.RequestFetchInventoryMetadata(barcode3);

            var messages = await RabbitTestHelper.WaitForMessagesAsync<FetchInventoryMetadataEvent>(rabbitChannel, queueName, 3);

            Assert.That(messages, Is.Not.Null);
            Assert.That(messages.Count, Is.EqualTo(3));

            Assert.That(messages[0].Barcode, Is.EqualTo(barcode1));
            Assert.That(messages[1].Barcode, Is.EqualTo(barcode2));
            Assert.That(messages[2].Barcode, Is.EqualTo(barcode3));
        }

        [Test]
        public async Task When_publishing_a_fetch_inventory_metadata_event_and_rabbit_restarts()
        {
            var barcode = "45612345685";

            await publisher.RequestFetchInventoryMetadata(barcode);

            await RabbitTestHelper.RestartRabbitMqAsync(
                containerName: "rabbitmq", 
                settings.GetRabbitMqSettings().HostName, 
                settings.GetRabbitMqSettings().AmqpPort,
                settings.GetRabbitMqSettings().ManagementPort,
                settings.GetRabbitMqSettings().UserName,
                settings.GetRabbitMqSettings().Password
            );

            testHelper = new IntegrationTestHelper(withRabbit: true);
            rabbitChannel = testHelper.rabbitChannel;
            rabbitConnection = testHelper.rabbitConnection;

            var messages = await RabbitTestHelper.WaitForMessagesAsync<FetchInventoryMetadataEvent>(rabbitChannel, queueName, 1);

            Assert.That(messages, Is.Not.Null);
            Assert.That(messages.Count, Is.EqualTo(1));
            Assert.That(messages.First().Barcode, Is.EqualTo(barcode));
        }

        [Test]
        public async Task When_publishing_a_fetch_inventory_metadata_event_and_rabbit_is_unreachable()
        {
            var barcode = "45689155685";
            var blackholeProvider = await testHelper.SpinUpBlackHoleRabbit(startHost: true);
            var publisher = blackholeProvider.GetRequiredService<IFetchInventoryMetadataRequestPublisher>();

            var result = await publisher.RequestFetchInventoryMetadata(barcode);

            Assert.That(result.Status, Is.EqualTo(PublisherResponseStatus.Error));
            Assert.That(result.Data.Count, Is.EqualTo(1));
            Assert.That(result.Data.First(), Is.TypeOf<FetchInventoryMetadataEvent>());
            Assert.That(((FetchInventoryMetadataEvent)result.Data.First()).Barcode, Is.EqualTo(barcode));
            Assert.That(result.Errors.Count, Is.EqualTo(1));
            Assert.That(result.Errors.First().StartsWith("RabbitMQ Error: Unable to reach rabbit host. Message: "));
        }
    }
}
