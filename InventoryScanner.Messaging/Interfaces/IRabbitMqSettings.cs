namespace InventoryScanner.Messaging.Interfaces
{
    public interface IRabbitMqSettings
    {
        public string HostName { get; }
        public string UserName { get; }
        public string Password { get; }
        public int AmqpPort { get; }
        public int ManagementPort { get; }
        public int PublishRetryCount { get; }
        public int ConnectionTimeout { get; }
        public string ExchangeName { get; }
        public string QueueName { get; }

    }
}
