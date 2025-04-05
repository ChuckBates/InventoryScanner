using InventoryScanner.Messaging.Interfaces;

namespace InventoryScanner.Messaging.IntegrationTests.Constructs
{
    public class TestRabbitMqSettings : IRabbitMqSettings
    {
        public required string HostName { get; set; }
        public int AmqpPort { get; set; }
        public int ManagementPort { get; set; }
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public int PublishRetryCount { get; set; }
        public int ConnectionTimeout { get; set; }
        public string? MessagingIntegrationTestQueueName { get; set; }
        public string? MessagingIntegrationTestExchangeName { get; set; }
        public string ExchangeName => MessagingIntegrationTestExchangeName ?? string.Empty;
        public string QueueName => MessagingIntegrationTestQueueName ?? string.Empty;
    }
}
