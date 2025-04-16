using InventoryScanner.Messaging.Interfaces;
using InventoryScanner.Messaging.Models;
using System.Text;
using Polly;
using EasyNetQ;
using System.Runtime.CompilerServices;
using InventoryScanner.Logging;

[assembly: InternalsVisibleTo("InventoryScanner.Messaging.IntegrationTests")]

namespace InventoryScanner.Messaging.Publishing
{
    public class RabbitMqPublisher : IRabbitMqPublisher
    {
        private readonly IRabbitMqSettings settings;
        private readonly IBus bus;
        private readonly IAppLogger<RabbitMqPublisher> logger;

        public RabbitMqPublisher(IRabbitMqSettings settings, IBus bus, IAppLogger<RabbitMqPublisher> logger)
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
                        logger.Error(ex, new LogContext
                        {
                            Barcode = null,
                            Component = typeof(RabbitMqPublisher).Name,
                            Message = $"Error occurred while publishing message to RabbitMQ. Retrying in {ts.TotalSeconds} seconds...",
                            Operation = "Publish"
                        });
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

                    var serializedMessage = System.Text.Json.JsonSerializer.Serialize(message);
                    var body = Encoding.UTF8.GetBytes(serializedMessage);
                    var properties = new MessageProperties
                    {
                        ContentType = "application/json",
                        DeliveryMode = 2,
                        Type = typeof(TEvent).FullName,
                        MessageId = Guid.NewGuid().ToString()
                    };

                    logger.Info(new LogContext
                    {
                        Barcode = null,
                        Component = typeof(RabbitMqPublisher).Name,
                        Message = $"Publishing message to RabbitMQ exchange {exchangeName}: {serializedMessage}",
                        Operation = "Execute"
                    });
                    await bus.Advanced.PublishAsync(exchange, string.Empty, false, properties, body);
                }
                catch (Exception ex)
                {
                    var errorMessage = $"RabbitMQ Error: Unable to reach rabbit host. Message: {ex.Message}";
                    logger.Error(ex, new LogContext
                    {
                        Barcode = null,
                        Component = typeof(RabbitMqPublisher).Name,
                        Message = "RabbitMQ Error: Unable to reach rabbit host.",
                        Operation = "Publish"
                    });
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