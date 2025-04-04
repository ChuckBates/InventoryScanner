using InventoryScanner.Messaging.Interfaces;
using InventoryScanner.Messaging.Models;
using System.Text;
using Polly;
using EasyNetQ;

namespace InventoryScanner.Messaging.Implementation
{
    public class RabbitMqPublisher : IRabbitMqPublisher
    {
        private readonly IRabbitMqSettings settings;
        private readonly IBus bus;

        public RabbitMqPublisher(IRabbitMqSettings settings, IBus bus)
        {
            this.settings = settings;
            this.bus = bus;
        }

        public async Task<PublisherResponse> PublishAsync<TEvent>(TEvent message) where TEvent : class, IRabbitMqEvent
        {
            var response = PublisherResponse.Success([message]);

            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: settings.PublishRetryCount,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (ex, ts) =>
                    {
                        Console.WriteLine($"[Retry] {ex.GetType().Name}: {ex.Message}");
                    });

            response = await retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    var exchange = await bus.Advanced.ExchangeDeclareAsync(
                        settings.FetchInventoryMetadataExchangeName,
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
                }
                catch (Exception ex)
                {
                    var meesage = $"RabbitMQ Error: Unable to reach rabbit host. Message: {ex.Message}";
                    return PublisherResponse.Failed([meesage], [message]);
                }

                return response;
            });

            return response;
        }
    }
}