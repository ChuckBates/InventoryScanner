using System.Net.WebSockets;

namespace InventoryScanner.Core.Lookups
{
    public class WebSocketWrapper : IWebSocketWrapper
    {
        private readonly WebSocket webSocket;
        public WebSocketState state => webSocket.State;

        public WebSocketWrapper(WebSocket webSocket)
        {
            this.webSocket = webSocket;
        }

        public async Task Send(string message)
        {
            if (webSocket.State == WebSocketState.Open)
            {
                var buffer = System.Text.Encoding.UTF8.GetBytes(message);
                var segment = new ArraySegment<byte>(buffer);
                await webSocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
            }
            else
            {
                throw new InvalidOperationException("WebSocket is not open.");
            }
        }
    }
}
