namespace InventoryScannerCore.Settings
{
    public class RabbitMqSettings
    {
        public string HostName { get; set; }
        public int AmqpPort { get; set; }
        public int ManagementPort { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FetchInventoryMetadataQueueName { get; set; }
        public string FetchInventoryMetadataExchangeName { get; set; }
        public int PublishRetryCount { get; set; }
        public int ConnectionTimeout { get; set; }
    }
}
