using InventoryScanner.Core.Handlers;
using InventoryScanner.Core.Wrappers;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;

namespace InventoryScanner.Core.Controllers
{
    [ApiController]
    [Route("inventory/updates")]
    public class InventoryUpdatesWebsocketController : ControllerBase
    {
        private readonly IInventoryUpdatesWebsocketHandler websocketHandler;

        public InventoryUpdatesWebsocketController(IInventoryUpdatesWebsocketHandler websocketHandler)
        {
            this.websocketHandler = websocketHandler;
        }

        [HttpGet]
        public async Task<IActionResult> Connect([FromQuery] string clientId)
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                return BadRequest("WebSocket request expected.");
            }

            var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            if (webSocket == null)
            {
                return BadRequest("Failed to accept WebSocket.");
            }

            if (string.IsNullOrEmpty(clientId))
            {
                return BadRequest("Client ID is required.");
            }

            var webSocketWrapper = new WebSocketWrapper(webSocket);
            websocketHandler.Register(clientId, webSocketWrapper);

            var buffer = new byte[1024 * 4];
            while (webSocketWrapper.state == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    websocketHandler.Unregister(clientId);
                }
            }

            return new EmptyResult();
        }
    }
}
