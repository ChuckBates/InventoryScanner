using InventoryScanner.Core.Handlers;
using InventoryScanner.Core.Lookups;
using InventoryScanner.Core.Messages;
using InventoryScanner.Core.Models;
using InventoryScanner.Logging;
using InventoryScanner.TestUtilities;
using Moq;
using System.Net.WebSockets;
using System.Text.Json;

namespace InventoryScanner.Core.UnitTests
{
    [TestFixture]
    public class InventoryUpdatesWebsocketHandlerTests
    {
        private Mock<IWebSocketWrapper> mockWebSocketWrapper1;
        private Mock<IWebSocketWrapper> mockWebSocketWrapper2;
        private Mock<IAppLogger<InventoryUpdatesWebsocketHandler>> mockLogger;
        private InventoryUpdatesWebsocketHandler handler;

        [SetUp]
        public void SetUp()
        {
            mockWebSocketWrapper1 = new Mock<IWebSocketWrapper>();
            mockWebSocketWrapper2 = new Mock<IWebSocketWrapper>();
            mockLogger = new Mock<IAppLogger<InventoryUpdatesWebsocketHandler>>();
            handler = new InventoryUpdatesWebsocketHandler(mockLogger.Object);
        }

        [Test]
        public async Task When_calling_broadcast_to_one_socket_successfully()
        {
            var clientId = Guid.NewGuid().ToString();

            handler.Register(clientId, mockWebSocketWrapper1.Object);

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

            mockWebSocketWrapper1.Setup(x => x.state).Returns(WebSocketState.Open);
            mockWebSocketWrapper1.Setup(x => x.Send(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            await handler.Broadcast(serializedMessage);

            Assert.That(handler.ConnectionKeys(), Has.Count.EqualTo(1));
            Assert.That(handler.ConnectionKeys(), Has.Member(clientId));

            mockWebSocketWrapper1.Verify(x => x.state, Times.Once);
            mockWebSocketWrapper1.Verify(x => x.Send(serializedMessage), Times.Once);
            mockLogger.Verify(
                x => x.Info(
                It.Is<LogContext>(l =>
                        l.Message == $"Sending serialized message to websocket {clientId}: {serializedMessage}" &&
                        l.Component == typeof(InventoryUpdatesWebsocketHandler).Name &&
                        l.Operation == "Send")
                ),
                Times.Once);
        }

        [Test]
        public async Task When_calling_broadcast_to_many_sockets_successfully()
        {
            var clientId1 = Guid.NewGuid().ToString();
            var clientId2 = Guid.NewGuid().ToString();
            
            handler.Register(clientId1, mockWebSocketWrapper1.Object);
            handler.Register(clientId2, mockWebSocketWrapper2.Object);

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

            mockWebSocketWrapper1.Setup(x => x.state).Returns(WebSocketState.Open);
            mockWebSocketWrapper1.Setup(x => x.Send(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            mockWebSocketWrapper2.Setup(x => x.state).Returns(WebSocketState.Open);
            mockWebSocketWrapper2.Setup(x => x.Send(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            await handler.Broadcast(serializedMessage);

            Assert.That(handler.ConnectionKeys(), Has.Count.EqualTo(2));
            Assert.That(handler.ConnectionKeys(), Has.Member(clientId1));
            Assert.That(handler.ConnectionKeys(), Has.Member(clientId2));

            mockWebSocketWrapper1.Verify(x => x.state, Times.Once);
            mockWebSocketWrapper1.Verify(x => x.Send(serializedMessage), Times.Once);
            mockLogger.Verify(
                x => x.Info(
                It.Is<LogContext>(l =>
                        l.Message == $"Sending serialized message to websocket {clientId1}: {serializedMessage}" &&
                        l.Component == typeof(InventoryUpdatesWebsocketHandler).Name &&
                        l.Operation == "Send")
                ),
                Times.Once);

            mockWebSocketWrapper2.Verify(x => x.state, Times.Once);
            mockWebSocketWrapper2.Verify(x => x.Send(serializedMessage), Times.Once);
            mockLogger.Verify(
                x => x.Info(
                It.Is<LogContext>(l =>
                        l.Message == $"Sending serialized message to websocket {clientId2}: {serializedMessage}" &&
                        l.Component == typeof(InventoryUpdatesWebsocketHandler).Name &&
                        l.Operation == "Send")
                ),
                Times.Once);
        }

        [Test]
        public async Task When_calling_broadcast_to_many_sockets_and_one_is_not_open()
        {
            var clientId1 = Guid.NewGuid().ToString();
            var clientId2 = Guid.NewGuid().ToString();

            handler.Register(clientId1, mockWebSocketWrapper1.Object);
            handler.Register(clientId2, mockWebSocketWrapper2.Object);

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

            mockWebSocketWrapper1.Setup(x => x.state).Returns(WebSocketState.Open);
            mockWebSocketWrapper1.Setup(x => x.Send(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            mockWebSocketWrapper2.Setup(x => x.state).Returns(WebSocketState.Closed);

            Assert.That(handler.ConnectionKeys, Has.Count.EqualTo(2));
            Assert.That(handler.ConnectionKeys(), Has.Member(clientId1));
            Assert.That(handler.ConnectionKeys(), Has.Member(clientId2));

            await handler.Broadcast(serializedMessage);

            Assert.That(handler.ConnectionKeys, Has.Count.EqualTo(1));
            Assert.That(handler.ConnectionKeys(), Has.Member(clientId1));
            Assert.That(handler.ConnectionKeys(), !Has.Member(clientId2));

            mockWebSocketWrapper1.Verify(x => x.state, Times.Once);
            mockWebSocketWrapper1.Verify(x => x.Send(serializedMessage), Times.Once);
            mockLogger.Verify(
                x => x.Info(
                It.Is<LogContext>(l =>
                        l.Message == $"Sending serialized message to websocket {clientId1}: {serializedMessage}" &&
                        l.Component == typeof(InventoryUpdatesWebsocketHandler).Name &&
                        l.Operation == "Send")
                ),
                Times.Once);

            mockWebSocketWrapper2.Verify(x => x.state, Times.Exactly(2));
            mockWebSocketWrapper2.Verify(x => x.Send(It.IsAny<string>()), Times.Never);
            mockLogger.Verify(
                x => x.Warning(
                It.Is<LogContext>(l =>
                        l.Message == $"Unable to send message to websocket {clientId2}: Socket Not Open. Current state: Closed" &&
                        l.Component == typeof(InventoryUpdatesWebsocketHandler).Name &&
                        l.Operation == "Send")
                ),
                Times.Once);
        }

        [Test]
        public async Task When_calling_broadcast_to_many_sockets_that_are_open_but_the_first_throws()
        {
            var clientId1 = Guid.NewGuid().ToString();
            var clientId2 = Guid.NewGuid().ToString();

            handler.Register(clientId1, mockWebSocketWrapper1.Object);
            handler.Register(clientId2, mockWebSocketWrapper2.Object);

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

            mockWebSocketWrapper1.Setup(x => x.state).Returns(WebSocketState.Open);
            mockWebSocketWrapper1.Setup(x => x.Send(It.IsAny<string>()))
                .Throws(new Exception("Broken pipe exception"));

            mockWebSocketWrapper2.Setup(x => x.state).Returns(WebSocketState.Open);
            mockWebSocketWrapper2.Setup(x => x.Send(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            await handler.Broadcast(serializedMessage);

            Assert.That(handler.ConnectionKeys(), Has.Count.EqualTo(1));
            Assert.That(handler.ConnectionKeys(), !Has.Member(clientId1));
            Assert.That(handler.ConnectionKeys(), Has.Member(clientId2));

            mockWebSocketWrapper1.Verify(x => x.state, Times.Once);
            mockWebSocketWrapper1.Verify(x => x.Send(serializedMessage), Times.Once);
            mockLogger.Verify(
                x => x.Warning(
                It.Is<LogContext>(l =>
                        l.Message == $"Unable to send message to websocket {clientId1}: Socket Pipe Broken." &&
                        l.Component == typeof(InventoryUpdatesWebsocketHandler).Name &&
                        l.Operation == "Send")
                ),
                Times.Once);
        }
    }
}
