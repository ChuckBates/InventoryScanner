using InventoryScanner.Core.Handlers;
using InventoryScanner.Core.Messages;
using InventoryScanner.Core.Models;
using InventoryScanner.Logging;
using InventoryScanner.TestUtilities;
using Moq;
using System.Text.Json;

namespace InventoryScanner.Core.UnitTests
{
    [TestFixture]
    public class InventoryUpdatedMessageHandlerTests
    {
        private Mock<IInventoryUpdatesWebsocketHandler> mockWebsocketHandler;
        private Mock<IAppLogger<InventoryUpdatedMessageHandler>> mockLogger;
        private InventoryUpdatedMessageHandler handler;

        [SetUp]
        public void SetUp()
        {
            mockWebsocketHandler = new Mock<IInventoryUpdatesWebsocketHandler>();
            mockLogger = new Mock<IAppLogger<InventoryUpdatedMessageHandler>>();
            handler = new InventoryUpdatedMessageHandler(mockWebsocketHandler.Object, mockLogger.Object);
        }

        [Test]
        public async Task When_calling_handle_successfully()
        {
            var barcode = Barcodes.Generate();
            var message = new InventoryUpdatedMessage
            {
                Barcode = barcode,
                MessageId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                UpdatedInventory = new Inventory
                {
                    Barcode = barcode,
                    Title = "test-title",
                    Description = "test-description",
                    Quantity = 10,
                    ImagePath = "test-image-path",
                    Categories = new List<string> { "test-category" },
                    UpdatedAt = DateTime.UtcNow
                }
            };
            var serializedMessage = JsonSerializer.Serialize(message);

            mockWebsocketHandler.Setup(x => x.Broadcast(It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            mockLogger.Setup(x => x.Info(It.IsAny<LogContext>()));

            await handler.Handle(message);

            mockWebsocketHandler.Verify(x => x.Broadcast(serializedMessage), Times.Once);
            mockLogger.Verify(
                x => x.Info(
                    It.Is<LogContext>(l =>
                        l.Barcode == barcode &&
                        l.Message == "Broadcasting InventoryUpdatedMessage to websocket." &&
                        l.Component == typeof(InventoryUpdatedMessageHandler).Name &&
                        l.Operation == "Broadcast")
                ),
                Times.Once);
        }

        [Test]
        public void When_calling_handle_and_the_broadcast_throws()
        {
            var barcode = Barcodes.Generate();
            var message = new InventoryUpdatedMessage
            {
                Barcode = barcode,
                MessageId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                UpdatedInventory = new Inventory
                {
                    Barcode = barcode,
                    Title = "test-title",
                    Description = "test-description",
                    Quantity = 10,
                    ImagePath = "test-image-path",
                    Categories = new List<string> { "test-category" },
                    UpdatedAt = DateTime.UtcNow
                }
            };
            var serializedMessage = JsonSerializer.Serialize(message);
            var broadcastException = new Exception("Broadcast error");

            mockWebsocketHandler.Setup(x => x.Broadcast(It.IsAny<string>()))
                .Throws(broadcastException);
            mockLogger.Setup(x => x.Error(It.IsAny<Exception>(), It.IsAny<LogContext>()));

            Assert.ThrowsAsync<Exception>(async () => await handler.Handle(message));

            mockWebsocketHandler.Verify(x => x.Broadcast(serializedMessage), Times.Once);
            mockLogger.Verify(
                x => x.Error(
                    It.Is<Exception>(e => e == broadcastException),
                    It.Is<LogContext>(l =>
                        l.Barcode == barcode &&
                        l.Message == "Error broadcasting InventoryUpdatedMessage to websocket." &&
                        l.Component == typeof(InventoryUpdatedMessageHandler).Name &&
                        l.Operation == "Broadcast")
                ),
                Times.Once);
        }
    }
}
