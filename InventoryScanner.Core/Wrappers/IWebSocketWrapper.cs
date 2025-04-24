using System.Net.WebSockets;

namespace InventoryScanner.Core.Wrappers
{
    public interface IWebSocketWrapper
    {
        WebSocketState state { get; }

        Task Send(string message);
    }
}