using InventoryScanner.Core.Wrappers;

namespace InventoryScanner.Core.Handlers
{
    public interface IInventoryUpdatesWebsocketHandler
    {
        Task Broadcast(string message);
        void Register(string clientId, IWebSocketWrapper socket);
        void Unregister(string clientId);
    }
}