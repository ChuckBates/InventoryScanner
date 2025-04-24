using System.Net.WebSockets;

namespace InventoryScanner.Core.Lookups
{
    public interface IWebSocketWrapper
    {
        WebSocketState state { get; }

        Task Send(string message);
    }
}