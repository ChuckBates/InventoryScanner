
using InventoryScanner.Core.Lookups;

namespace InventoryScanner.Core.Handlers
{
    public interface IInventoryUpdatesWebsocketHandler
    {
        Task Broadcast(string message);
        void Register(string clientId, IWebSocketWrapper socket);
        void Unregister(string clientId);
    }
}