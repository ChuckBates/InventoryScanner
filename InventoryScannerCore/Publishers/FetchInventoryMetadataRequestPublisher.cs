using EasyNetQ;
using InventoryScannerCore.Enums;
using InventoryScannerCore.Events;
using InventoryScannerCore.Settings;
using Polly;
using RabbitMQ.Client.Exceptions;
using System.Text;
using System.Text.Json;

namespace InventoryScannerCore.Publishers
{
    public class FetchInventoryMetadataRequestPublisher : IFetchInventoryMetadataRequestPublisher
    {
        private readonly IBus bus;
        private readonly ISettingsService settingsService;
        private readonly AsyncPolicy retryPolicy;

        public FetchInventoryMetadataRequestPublisher(IBus bus, ISettingsService settingsService)
        {
            this.bus = bus;
            this.settingsService = settingsService;
            retryPolicy = Policy
                .Handle<Exception>(ex => IsTransient(ex))
                .WaitAndRetryAsync(
                    retryCount: settingsService.GetRabbitMqSettings().PublishRetryCount,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(2),
                    onRetry: (ex, timespan) =>
                    {
                        Console.WriteLine($"Retrying due to: {ex.Message}");
                    });
        }

        public async Task<PublisherResponse> RequestFetchInventoryMetadata(string barcode)
        {
            var message = new FetchInventoryMetadataEvent
            {
                Barcode = barcode,
                EventId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow
            };
            var response = new PublisherResponse(PublisherResponseStatus.Success, [message], []);

            try
            {
                await retryPolicy.ExecuteAsync(async () =>
                {
                    var exchange = await bus.Advanced.ExchangeDeclareAsync(
                        settingsService.GetRabbitMqSettings().FetchInventoryMetadataExchangeName,
                        type: "fanout",
                        durable: true,
                        autoDelete: false);

                    var body = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(message));
                    var properties = new MessageProperties
                    {
                        ContentType = "application/json",
                        DeliveryMode = 2
                    };

                    await bus.Advanced.PublishAsync(exchange, string.Empty, false, properties, body);
                });
            }
            catch (Exception e)
            {
                Console.WriteLine($"RabbitMQ Error: Unable to reach rabbit host. Message: {e.Message}");
                response.Status = PublisherResponseStatus.Error;
                response.Errors.Add("RabbitMQ Error: Unable to reach rabbit host. Message: " + e.Message);
            }

            return response;
        }
        private static bool IsTransient(Exception ex)
        {
            if (ex is EasyNetQException) return true;

            if (ex.InnerException is RabbitMQ.Client.Exceptions.BrokerUnreachableException)
                return true;

            if (ex.InnerException is System.IO.EndOfStreamException)
                return false; // This indicates permanent closure

            if (ex is AlreadyClosedException)
                return false;

            if (ex.Message.Contains("End of stream") || ex.Message.Contains("Already closed"))
                return false;

            return true;
        }
    }
}
