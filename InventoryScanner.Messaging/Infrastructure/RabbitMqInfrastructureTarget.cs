namespace InventoryScanner.Messaging.Infrastructure
{
    public class RabbitMqInfrastructureTarget
    {
        public required string ExchangeName { get; set; }
        public required string QueueName { get; set; }
        public string ExchangeType { get; set; } = "fanout";
        public string RoutingKey { get; set; } = string.Empty;
    }
}
