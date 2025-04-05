using InventoryScanner.Messaging.Interfaces;

namespace InventoryScanner.Core.Settings
{
    public class RabbitMqSettings : IRabbitMqSettings
    {
        public required string HostName { get; set; }
        public int AmqpPort { get; set; }
        public int ManagementPort { get; set; }
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public string? FetchInventoryMetadataQueueName { get; set; }
        public string? FetchInventoryMetadataExchangeName { get; set; }
        public int PublishRetryCount { get; set; }
        public int ConnectionTimeout { get; set; }
        public string ExchangeName => FetchInventoryMetadataExchangeName ?? string.Empty;
        public string QueueName => FetchInventoryMetadataQueueName ?? string.Empty;
    }
}
