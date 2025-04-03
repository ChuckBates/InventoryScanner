using InventoryScannerCore.Enums;
using InventoryScannerCore.Events;
using InventoryScannerCore.Publishers;
using InventoryScannerCore.Settings;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace InventoryScannerCore.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public class FetchInventoryMetadataRequestPublisherTests
    {
        string amqpProxyName;
        string managementProxyName;

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            await ToxiProxyHelper.ClearAllProxiesAsync();

            amqpProxyName = "rabbitmq-amqp";
            var amqpProxyListenAt = "0.0.0.0:5673";
            var amqpProxyUpstreamAt = "rabbitmq:5672";
            await ToxiProxyHelper.CreateProxyAsync(amqpProxyName, amqpProxyListenAt, amqpProxyUpstreamAt);

            managementProxyName = "rabbitmq-management";
            var managementProxyListenAt = "0.0.0.0:15673";
            var managementProxyUpstreamAt = "rabbitmq:15672";
            await ToxiProxyHelper.CreateProxyAsync(managementProxyName, managementProxyListenAt, managementProxyUpstreamAt);
        }

        [OneTimeTearDown]
        public async Task TearDown()
        {
            await ToxiProxyHelper.ClearAllProxiesAsync();
        }

        [Test]
        public async Task When_publishing_a_fetch_inventory_metadata_event()
        {
            var testHelper = new IntegrationTestDependencyHelper();
            await testHelper.SpinUp(withRabbit: true);

            var publisher = testHelper.provider.GetRequiredService<IFetchInventoryMetadataRequestPublisher>();
            var rabbitChannel = testHelper.rabbitChannel;
            var rabbitConnection = testHelper.rabbitConnection;
            var settings = testHelper.provider.GetRequiredService<ISettingsService>();
            var queueName = settings.GetRabbitMqSettings().FetchInventoryMetadataQueueName;

            await PurgeQueue(rabbitChannel, rabbitConnection, queueName, "When_publishing_a_fetch_inventory_metadata_event:100");

            var barcode = "1234567890";

            await publisher.RequestFetchInventoryMetadata(barcode);

            var messages = await RabbitTestHelper.WaitForMessagesAsync<FetchInventoryMetadataEvent>(rabbitChannel, queueName, 1);

            Assert.That(messages, Is.Not.Null);
            Assert.That(messages.Count, Is.EqualTo(1));
            Assert.That(messages.First().Barcode, Is.EqualTo(barcode));
            Assert.That(Guid.TryParse(messages.First().EventId, out _), Is.True);
            Assert.That((DateTime.UtcNow - messages.First().Timestamp).TotalSeconds, Is.LessThan(5));

            await PurgeQueue(rabbitChannel, rabbitConnection, queueName, "When_publishing_a_fetch_inventory_metadata_event:114");

            rabbitChannel.Close();
            rabbitConnection.Close();
        }

        [Test]
        public async Task When_publishing_multiple_fetch_inventory_metadata_events()
        {
            var testHelper = new IntegrationTestDependencyHelper();
            await testHelper.SpinUp(withRabbit: true);

            var publisher = testHelper.provider.GetRequiredService<IFetchInventoryMetadataRequestPublisher>();
            var rabbitChannel = testHelper.rabbitChannel;
            var rabbitConnection = testHelper.rabbitConnection;
            var settings = testHelper.provider.GetRequiredService<ISettingsService>();
            var queueName = settings.GetRabbitMqSettings().FetchInventoryMetadataQueueName;
            await PurgeQueue(rabbitChannel, rabbitConnection, queueName, "When_publishing_multiple_fetch_inventory_metadata_events:132");

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

            await PurgeQueue(rabbitChannel, rabbitConnection, queueName, "When_publishing_multiple_fetch_inventory_metadata_events:151");

            rabbitChannel.Close();
            rabbitConnection.Close();
        }

        [Test]
        public async Task When_publishing_a_fetch_inventory_metadata_event_and_rabbit_is_unreachable()
        {
            var testHelper = new IntegrationTestDependencyHelper();
            await testHelper.SpinUp(withRabbit: true);

            var publisher = testHelper.provider.GetRequiredService<IFetchInventoryMetadataRequestPublisher>();
            var rabbitChannel = testHelper.rabbitChannel;
            var rabbitConnection = testHelper.rabbitConnection;
            var settings = testHelper.provider.GetRequiredService<ISettingsService>();
            var queueName = settings.GetRabbitMqSettings().FetchInventoryMetadataQueueName;

            await PurgeQueue(rabbitChannel, rabbitConnection, queueName, "When_publishing_a_fetch_inventory_metadata_event_and_rabbit_is_unreachable:169");

            var barcode = "45689155685";
            var toxicName = "toxic-rabbit-amqp-unreachable";

            await ToxiProxyHelper.CutConnectionAsync(amqpProxyName, toxicName);
            await Task.Delay(1000);

            var result = await publisher.RequestFetchInventoryMetadata(barcode);

            Assert.That(result.Status, Is.EqualTo(PublisherResponseStatus.Error));
            Assert.That(result.Data.Count, Is.EqualTo(1));
            Assert.That(result.Data.First(), Is.TypeOf<FetchInventoryMetadataEvent>());
            Assert.That(((FetchInventoryMetadataEvent)result.Data.First()).Barcode, Is.EqualTo(barcode));
            Assert.That(result.Errors.Count, Is.EqualTo(1));
            Assert.That(result.Errors.First().StartsWith("RabbitMQ Error: Unable to reach rabbit host. Message: "));

            await ToxiProxyHelper.RestoreConnectionAsync(amqpProxyName, toxicName);

            await PurgeQueue(rabbitChannel, rabbitConnection, queueName, "When_publishing_a_fetch_inventory_metadata_event_and_rabbit_is_unreachable:192");

            rabbitChannel.Close();
            rabbitConnection.Close();
        }

        [Test]
        public async Task When_publishing_a_fetch_inventory_metadata_event_and_the_publish_fails_transiently()
        {
            var testHelper = new IntegrationTestDependencyHelper();
            await testHelper.SpinUp(withRabbit: true);

            var publisher = testHelper.provider.GetRequiredService<IFetchInventoryMetadataRequestPublisher>();
            var rabbitChannel = testHelper.rabbitChannel;
            var rabbitConnection = testHelper.rabbitConnection;
            var settings = testHelper.provider.GetRequiredService<ISettingsService>();
            var queueName = settings.GetRabbitMqSettings().FetchInventoryMetadataQueueName;

            await PurgeQueue(rabbitChannel, rabbitConnection, queueName, "When_publishing_a_fetch_inventory_metadata_event_and_the_publish_fails_transiently:210");

            var barcode = "45689155685";
            var toxicName = "toxic-rabbit-amqp-transient";

            await ToxiProxyHelper.CutConnectionAsync(amqpProxyName, toxicName);

            var result = await publisher.RequestFetchInventoryMetadata(barcode);

            await Task.Delay(2000);
            await ToxiProxyHelper.RestoreConnectionAsync(amqpProxyName, toxicName);

            Assert.That(result.Status, Is.EqualTo(PublisherResponseStatus.Success));
            Assert.That(result.Data.Count, Is.EqualTo(1));
            Assert.That(result.Data.First(), Is.TypeOf<FetchInventoryMetadataEvent>());
            Assert.That(((FetchInventoryMetadataEvent)result.Data.First()).Barcode, Is.EqualTo(barcode));
            Assert.That(result.Errors.Count, Is.EqualTo(0));

            await PurgeQueue(rabbitChannel, rabbitConnection, queueName, "When_publishing_a_fetch_inventory_metadata_event_and_the_publish_fails_transiently:228");

            rabbitChannel.Close();
        }

        private async Task<string> GetHttp(string uri)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            };
        }

        private async Task PurgeQueue(IModel rabbitChannel, IConnection rabbitConnection, string queueName, string location)
        {
            //await Task.Delay(3000);
            Console.WriteLine($"Attempting to purge queue {queueName} at {location}... (RabbitMQ connection: {rabbitConnection?.IsOpen}, channel: {rabbitChannel?.IsOpen})" +
                $" (RabbitMQ connection: {rabbitConnection?.Endpoint.HostName}:{rabbitConnection?.Endpoint.Port}, channel: {rabbitChannel?.ChannelNumber})" +
                $" (RabbitMQ queue: {queueName}, message count: {rabbitChannel?.MessageCount(queueName)})");

            try
            {
                if (rabbitChannel == null || !rabbitChannel.IsOpen)
                {
                    Console.WriteLine("RabbitMQ channel is not open.");
                    return;
                }

                if (rabbitConnection == null || !rabbitConnection.IsOpen)
                {
                    Console.WriteLine("RabbitMQ connection is not open.");
                    return;
                }

                var queueDeclareOk = rabbitChannel.QueueDeclarePassive(queueName);
                if (queueDeclareOk == null)
                {
                    Console.WriteLine($"Queue {queueName} does not exist.");
                    return;
                }

                var messageCountBeforePurge = queueDeclareOk.MessageCount;
                Console.WriteLine($"Queue {queueName} has {messageCountBeforePurge} messages before purge.");

                rabbitChannel.QueuePurge(queueName);
                await Task.Delay(1000);

                // Re-declare the queue to get the message count after purge
                queueDeclareOk = rabbitChannel.QueueDeclarePassive(queueName);
                var messageCountAfterPurge = queueDeclareOk.MessageCount;
                Console.WriteLine($"Queue {queueName} has {messageCountAfterPurge} messages after purge.");

                if (messageCountAfterPurge == 0)
                {
                    Console.WriteLine($"Queue {queueName} purged successfully.");
                }
                else
                {
                    Console.WriteLine($"Queue {queueName} still has {messageCountAfterPurge} messages after purge.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to purge queue {queueName}: {ex.Message}");
            }
        }
    }
}
