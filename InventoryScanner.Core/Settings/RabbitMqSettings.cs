﻿using InventoryScanner.Messaging.Interfaces;

namespace InventoryScanner.Core.Settings
{
    public class RabbitMqSettings : IRabbitMqSettings
    {
        public required string HostName { get; set; }
        public int AmqpPort { get; set; }
        public int ManagementPort { get; set; }
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public required string FetchInventoryMetadataQueueName { get; set; }
        public required string FetchInventoryMetadataExchangeName { get; set; }
        public required string FetchInventoryMetadataDeadLetterQueueName { get; set; }
        public required string FetchInventoryMetadataDeadLetterExchangeName { get; set; }
        public required string InventoryUpdatedQueueName { get; set; }
        public required string InventoryUpdatedExchangeName { get; set; }
        public required string InventoryUpdatedDeadLetterQueueName { get; set; }
        public required string InventoryUpdatedDeadLetterExchangeName { get; set; }
        public int PublishRetryCount { get; set; }
        public int SubscribeRetryCount { get; set; }
        public int ConnectionTimeout { get; set; }
        public string ExchangeName { get; set; } = string.Empty;
        public string QueueName { get; set; } = string.Empty;
    }
}
