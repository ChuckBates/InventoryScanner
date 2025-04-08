using RabbitMQ.Client;

namespace InventoryScanner.Messaging.Interfaces
{
    public interface IRabbitMqConnectionManager
    {
        Task<IConnection> GetConnectionAsync();
    }
}
