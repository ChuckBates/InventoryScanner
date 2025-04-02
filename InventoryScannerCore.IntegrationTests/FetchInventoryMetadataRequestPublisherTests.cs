using InventoryScannerCore.Events;
using InventoryScannerCore.Publishers;
using InventoryScannerCore.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace InventoryScannerCore.IntegrationTests
{
    [TestFixture]
    public class FetchInventoryMetadataRequestPublisherTests
    {
        IConnection rabbitConnection;
        IModel rabbitChannel;
        IFetchInventoryMetadataRequestPublisher publisher;
        ISettingsService settings;

        [SetUp]
        public void Setup()
        {
            var testHelper = new IntegrationTestHelper(withRabbit: true);
            publisher = testHelper.provider.GetRequiredService<IFetchInventoryMetadataRequestPublisher>();
            rabbitChannel = testHelper.rabbitChannel;
            rabbitConnection = testHelper.rabbitConnection;
            settings = testHelper.provider.GetRequiredService<ISettingsService>();
        }

        [TearDown]
        public void TearDown()
        {
            rabbitChannel.Close();
            rabbitConnection.Close();
        }

        [Test]
        public async Task When_publishing_a_fetch_inventory_metadata_event()
        {
            rabbitChannel.QueuePurge(settings.GetRabbitMqSettings().FetchInventoryMetadataQueueName);

            var barcode = "1234567890";

            await publisher.RequestFetchInventoryMetadata(barcode);

            var result = rabbitChannel.BasicGet(settings.GetRabbitMqSettings().FetchInventoryMetadataQueueName, true);

            Assert.That(result, Is.Not.Null);

            var body = Encoding.UTF8.GetString(result.Body.ToArray());
            var message = JsonSerializer.Deserialize<FetchInventoryMetadataEvent>(body);

            Assert.That(message, Is.Not.Null);
            Assert.That(message.Barcode, Is.EqualTo(barcode));
        }
    }
}
