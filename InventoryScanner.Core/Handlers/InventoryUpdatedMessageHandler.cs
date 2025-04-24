using InventoryScanner.Core.Messages;
using InventoryScanner.Logging;
using System.Text.Json;

namespace InventoryScanner.Core.Handlers
{
    public class InventoryUpdatedMessageHandler : IInventoryUpdatedMessageHandler
    {
        private readonly IInventoryUpdatesWebsocketHandler inventoryUpdatesWebsocketHandler;
        private readonly IAppLogger<InventoryUpdatedMessageHandler> logger;

        public InventoryUpdatedMessageHandler(IInventoryUpdatesWebsocketHandler inventoryUpdatesWebsocketHandler, IAppLogger<InventoryUpdatedMessageHandler> logger)
        {
            this.inventoryUpdatesWebsocketHandler = inventoryUpdatesWebsocketHandler;
            this.logger = logger;
        }

        public async Task Handle(InventoryUpdatedMessage message)
        {
            try
            {
                await inventoryUpdatesWebsocketHandler.Broadcast(JsonSerializer.Serialize(message));
                logger.Info(new LogContext
                {
                    Barcode = message.Barcode,
                    Message = "Broadcasting InventoryUpdatedMessage to websocket.",
                    Component = nameof(InventoryUpdatedMessageHandler),
                    Operation = "Broadcast"
                });
            }
            catch (Exception e)
            {
                logger.Error(e, new LogContext
                {
                    Barcode = message.Barcode,
                    Message = "Error broadcasting InventoryUpdatedMessage to websocket.",
                    Component = nameof(InventoryUpdatedMessageHandler),
                    Operation = "Broadcast"
                });

                throw;
            }
        }
    }
}
