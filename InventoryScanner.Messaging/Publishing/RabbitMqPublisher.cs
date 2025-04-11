using InventoryScanner.Messaging.Interfaces;
using InventoryScanner.Messaging.Models;
using System.Text;
using Polly;
using EasyNetQ;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("InventoryScanner.Messaging.IntegrationTests")]

namespace InventoryScanner.Messaging.Publishing
{
    public class RabbitMqPublisher : IRabbitMqPublisher
    {
        private readonly IRabbitMqSettings settings;
        private readonly IBus bus;
        private readonly ILogger<RabbitMqPublisher> logger;

        public RabbitMqPublisher(IRabbitMqSettings settings, IBus bus, ILogger<RabbitMqPublisher> logger)
        {
            this.settings = settings;
            this.bus = bus;
            this.logger = logger;
        }

        public async Task<PublisherResponse> PublishAsync<TEvent>(TEvent message, string exchangeName) where TEvent : class, IRabbitMqMessage
        {
            var response = PublisherResponse.Success([message]);

            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: settings.PublishRetryCount,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (ex, ts) =>
                    {
                        logger.LogError(ex, "Error occurred while publishing message to RabbitMQ. Retrying in {TotalSeconds} seconds...", ts.TotalSeconds);
                    });

            response = await retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    var exchange = await bus.Advanced.ExchangeDeclareAsync(
                        exchangeName,
                        type: "fanout",
                        durable: true,
                        autoDelete: false);

                    var body = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(message));
                    var properties = new MessageProperties
                    {
                        ContentType = "application/json",
                        DeliveryMode = 2,
                        Type = typeof(TEvent).FullName,
                        MessageId = Guid.NewGuid().ToString()
                    };

                    logger.LogInformation("Publishing message to RabbitMQ exchange {exchange}: {Message}", exchangeName, message);
                    await bus.Advanced.PublishAsync(exchange, string.Empty, false, properties, body);
                }
                catch (Exception ex)
                {
                    var errorMessage = $"RabbitMQ Error: Unable to reach rabbit host. Message: {ex.Message}";
                    logger.LogError(ex, "RabbitMQ Error: Unable to reach rabbit host. Message: {Message}", ex.Message);
                    return PublisherResponse.Failed([errorMessage], [message]);
                }

                return response;
            });

            return response;
        }

        internal async Task RawPublishAsync(string rawJson, string exchangeName)
        {
            var exchange = await bus.Advanced.ExchangeDeclareAsync(
                exchangeName,
                type: "fanout",
                durable: true,
                autoDelete: false);

            var body = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(rawJson));
            var properties = new MessageProperties
            {
                ContentType = "application/json",
                DeliveryMode = 2,
                MessageId = Guid.NewGuid().ToString()
            };

            await bus.Advanced.PublishAsync(exchange, string.Empty, false, properties, body);
        }
    }
}