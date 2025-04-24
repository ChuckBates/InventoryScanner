using InventoryScanner.Core.Wrappers;
using InventoryScanner.Logging;
using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace InventoryScanner.Core.Handlers
{
    public class InventoryUpdatesWebsocketHandler : IInventoryUpdatesWebsocketHandler
    {
        private readonly ConcurrentDictionary<string, IWebSocketWrapper> connections = new();
        private readonly IAppLogger<InventoryUpdatesWebsocketHandler> logger;
        public List<string> ConnectionKeys() => connections.Keys.ToList();

        public InventoryUpdatesWebsocketHandler(IAppLogger<InventoryUpdatesWebsocketHandler> logger)
        {
            this.logger = logger;
        }

        public void Register(string clientId, IWebSocketWrapper socket)
        {
            connections[clientId] = socket;
            logger.Info(new LogContext
            {
                Message = $"WebSocket {clientId} registered.",
                Component = nameof(InventoryUpdatesWebsocketHandler),
                Operation = "Register"
            });
        }

        public void Unregister(string clientId)
        {
            var removed = connections.TryRemove(clientId, out _);
            if (removed)
            {
                logger.Info(new LogContext
                {
                    Message = $"WebSocket {clientId} unregistered.",
                    Component = nameof(InventoryUpdatesWebsocketHandler),
                    Operation = "Unregister"
                });
            }
            else
            {
                logger.Warning(new LogContext
                {
                    Message = $"Failed to unregister WebSocket {clientId}.",
                    Component = nameof(InventoryUpdatesWebsocketHandler),
                    Operation = "Unregister"
                });
            }

        }

        public async Task Broadcast(string message)
        {
            var tasks = connections.Values.Select(async socket =>
            {
                var clientId = connections.FirstOrDefault(x => x.Value == socket).Key;
                if (socket.state == WebSocketState.Open)
                {
                    try
                    {
                        await socket.Send(message);
                    }
                    catch (Exception)
                    {
                        Unregister(clientId);
                        var errorMessage = $"Unable to send message to websocket {clientId}: Socket Pipe Broken.";
                        logger.Warning(new LogContext
                        {
                            Message = errorMessage,
                            Component = nameof(InventoryUpdatesWebsocketHandler),
                            Operation = "Send"
                        });
                        return;
                    }

                    logger.Info(new LogContext
                    {
                        Message = $"Sending serialized message to websocket {clientId}: {message}",
                        Component = nameof(InventoryUpdatesWebsocketHandler),
                        Operation = "Send"
                    });
                }
                else
                {
                    Unregister(clientId);
                    var errorMessage = $"Unable to send message to websocket {clientId}: Socket Not Open. Current state: {socket.state}";
                    logger.Warning(new LogContext
                    {
                        Message = errorMessage,
                        Component = nameof(InventoryUpdatesWebsocketHandler),
                        Operation = "Send"
                    });
                }
            });

            await Task.WhenAll(tasks);
        }
    }
}
