using InventoryScanner.Core.Handlers;
using InventoryScanner.Core.Lookups;
using InventoryScanner.Core.Messages;
using InventoryScanner.Core.Models;
using InventoryScanner.Core.Publishers.Interfaces;
using InventoryScanner.Core.Repositories;
using InventoryScanner.Logging;
using InventoryScanner.Messaging.Publishing;
using InventoryScanner.TestUtilities;
using Moq;

namespace InventoryScanner.Core.UnitTests
{
    [TestFixture]
    public class FetchInventoryMetadataMessageHandlerTests
    {
        private Mock<IBarcodeLookup> mockBarcodeLookup;
        private Mock<IImageLookup> mockImageLookup;
        private Mock<IImageRepository> mockImageRepository;
        private Mock<IInventoryRepository> mockInventoryRepository;
        private Mock<IInventoryUpdatedPublisher> mockInventoryUpdatedPublisher;
        private Mock<IAppLogger<FetchInventoryMetadataMessageHandler>> mockLogger;
        private FetchInventoryMetadataMessageHandler handler;

        [SetUp]
        public void SetUp()
        {
            mockBarcodeLookup = new Mock<IBarcodeLookup>();
            mockImageLookup = new Mock<IImageLookup>();
            mockImageRepository = new Mock<IImageRepository>();
            mockInventoryRepository = new Mock<IInventoryRepository>();
            mockInventoryUpdatedPublisher = new Mock<IInventoryUpdatedPublisher>();
            mockLogger = new Mock<IAppLogger<FetchInventoryMetadataMessageHandler>>();
            handler = new FetchInventoryMetadataMessageHandler(
                mockBarcodeLookup.Object,
                mockImageLookup.Object, 
                mockImageRepository.Object, 
                mockInventoryRepository.Object, 
                mockInventoryUpdatedPublisher.Object,
                mockLogger.Object);
        }

        [Test]
        public void When_calling_handle_successfully()
        {
            var barcode = Barcodes.Generate();
            var title = "Test-Product";
            var description = "Test-Description";
            var quantity = 1;
            var categories = new List<string> { "cat-1", "cat-2" };
            var imagePath = Directory.GetCurrentDirectory() + $"/Images/{title}-{barcode}.jpg";
            var imageUrl = "https://test.com/image1.jpg";
            var inventory = new Inventory
            {
                Barcode = barcode,
                Quantity = quantity,
                Categories = categories
            };
            var message = new FetchInventoryMetadataMessage
            { 
                Barcode = barcode,
                MessageId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow
            };
            var updatedInventory = new Inventory
            {
                Barcode = barcode,
                Title = title,
                Description = description,
                Quantity = quantity,
                ImagePath = imagePath,
                Categories = categories
            };
            var imageStream = new MemoryStream();
            var expectedPublisherData = new InventoryUpdatedMessage
            {
                Barcode = barcode,
                UpdatedInventory = updatedInventory,
                MessageId = message.MessageId,
                Timestamp = message.Timestamp
            };
            var expectedPublisherResponse = PublisherResponse.Success([expectedPublisherData]);

            mockInventoryRepository.Setup(x => x.Get(barcode)).ReturnsAsync(inventory);
            mockBarcodeLookup.Setup(x => x.Get(barcode)).ReturnsAsync(new Barcode
            {
                product = new BarcodeProduct
                {
                    barcode = inventory.Barcode,
                    title = title,
                    description = description,
                    images = [imageUrl]
                }
            });
            mockInventoryRepository.Setup(x => x.Get(barcode)).ReturnsAsync(inventory);
            mockImageLookup.Setup(x => x.Get(imageUrl)).ReturnsAsync(imageStream);
            mockImageRepository.Setup(x => x.Insert(imageStream, imagePath)).ReturnsAsync("success");
            mockInventoryRepository.Setup(x => x.Insert(updatedInventory)).ReturnsAsync(1);
            mockInventoryUpdatedPublisher.Setup(x => x.Publish(updatedInventory)).ReturnsAsync(expectedPublisherResponse);

            Assert.DoesNotThrowAsync(async () => await handler.Handle(message));

            mockInventoryRepository.Verify(x => x.Get(barcode), Times.Once);
            mockBarcodeLookup.Verify(x => x.Get(inventory.Barcode), Times.Once);
            mockImageLookup.Verify(x => x.Get(imageUrl), Times.Once);
            mockImageRepository.Verify(x => x.Insert(imageStream, imagePath), Times.Once);
            mockInventoryRepository.Verify(x => x.Insert(
                It.Is<Inventory>(x =>
                    x.Barcode == barcode &&
                    x.Title == title &&
                    x.Description == description &&
                    x.Quantity == quantity &&
                    x.ImagePath == imagePath &&
                    x.Categories.SequenceEqual(categories)
                )), Times.Once);
            mockInventoryUpdatedPublisher.Verify(x => x.Publish(updatedInventory), Times.Once);
        }

        [Test]
        public void When_calling_handle_and_the_inventory_updated_publish_fails()
        {
            var barcode = Barcodes.Generate();
            var title = "Test-Product";
            var description = "Test-Description";
            var quantity = 1;
            var categories = new List<string> { "cat-1", "cat-2" };
            var imagePath = Directory.GetCurrentDirectory() + $"/Images/{title}-{barcode}.jpg";
            var imageUrl = "https://test.com/image1.jpg";
            var inventory = new Inventory
            {
                Barcode = barcode,
                Quantity = quantity,
                Categories = categories
            };
            var message = new FetchInventoryMetadataMessage
            {
                Barcode = barcode,
                MessageId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow
            };
            var updatedInventory = new Inventory
            {
                Barcode = barcode,
                Title = title,
                Description = description,
                Quantity = quantity,
                ImagePath = imagePath,
                Categories = categories
            };
            var imageStream = new MemoryStream();
            var expectedPublisherData = new InventoryUpdatedMessage
            {
                Barcode = barcode,
                UpdatedInventory = updatedInventory,
                MessageId = message.MessageId,
                Timestamp = message.Timestamp
            };
            var expectedPublisherResponse = PublisherResponse.Failed(["error publishing message"], [expectedPublisherData]);

            mockInventoryRepository.Setup(x => x.Get(barcode)).ReturnsAsync(inventory);
            mockBarcodeLookup.Setup(x => x.Get(barcode)).ReturnsAsync(new Barcode
            {
                product = new BarcodeProduct
                {
                    barcode = inventory.Barcode,
                    title = title,
                    description = description,
                    images = [imageUrl]
                }
            });
            mockInventoryRepository.Setup(x => x.Get(barcode)).ReturnsAsync(inventory);
            mockImageLookup.Setup(x => x.Get(imageUrl)).ReturnsAsync(imageStream);
            mockImageRepository.Setup(x => x.Insert(imageStream, imagePath)).ReturnsAsync("success");
            mockInventoryRepository.Setup(x => x.Insert(updatedInventory)).ReturnsAsync(1);
            mockInventoryUpdatedPublisher.Setup(x => x.Publish(updatedInventory)).ReturnsAsync(expectedPublisherResponse);
            mockLogger.Setup(x => x.Warning(It.IsAny<LogContext>()));

            Assert.DoesNotThrowAsync(async () => await handler.Handle(message));

            mockInventoryRepository.Verify(x => x.Get(barcode), Times.Once);
            mockBarcodeLookup.Verify(x => x.Get(inventory.Barcode), Times.Once);
            mockImageLookup.Verify(x => x.Get(imageUrl), Times.Once);
            mockImageRepository.Verify(x => x.Insert(imageStream, imagePath), Times.Once);
            mockInventoryRepository.Verify(x => x.Insert(
                It.Is<Inventory>(x =>
                    x.Barcode == barcode &&
                    x.Title == title &&
                    x.Description == description &&
                    x.Quantity == quantity &&
                    x.ImagePath == imagePath &&
                    x.Categories.SequenceEqual(categories)
                )), Times.Once);
            mockInventoryUpdatedPublisher.Verify(x => x.Publish(updatedInventory), Times.Once);
            mockLogger.Verify(
                x => x.Warning(
                    It.Is<LogContext>(l => 
                        l.Barcode == barcode && 
                        l.Message == "Error handling metadata update message: Failed to publish inventory update." &&
                        l.Component == typeof(FetchInventoryMetadataMessageHandler).Name &&
                        l.Operation == "Fetch Details")
                ),
                Times.Once);
        }

        [Test]
        public void When_calling_handle_and_inventory_get_returns_nothing()
        {
            var barcode = Barcodes.Generate();
            var message = new FetchInventoryMetadataMessage
            {
                Barcode = barcode,
                MessageId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow
            };

            mockInventoryRepository.Setup(x => x.Get(barcode)).ReturnsAsync(null as Inventory);

            var exception = Assert.ThrowsAsync<Exception>(async () => await handler.Handle(message));

            Assert.That(exception.Message, Is.EqualTo("Error handling metadata update message: Inventory not found."));

            mockInventoryRepository.Verify(x => x.Get(barcode), Times.Once);
            mockBarcodeLookup.Verify(x => x.Get(It.IsAny<string>()), Times.Never);
            mockImageLookup.Verify(x => x.Get(It.IsAny<string>()), Times.Never);
            mockImageRepository.Verify(x => x.Insert(It.IsAny<MemoryStream>(), It.IsAny<string>()), Times.Never);
            mockInventoryRepository.Verify(x => x.Insert(It.IsAny<Inventory>()), Times.Never);
        }

        [Test]
        public void When_calling_handle_and_barcode_lookup_returns_no_images()
        {
            var barcode = Barcodes.Generate();
            var title = "Test-Product";
            var description = "Test-Description";
            var quantity = 1;
            var categories = new List<string>();
            var imagePath = "";
            var inventory = new Inventory
            {
                Barcode = barcode,
                Quantity = quantity
            };
            var message = new FetchInventoryMetadataMessage
            {
                Barcode = barcode,
                MessageId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow
            };

            var updatedInventory = new Inventory
            {
                Barcode = barcode,
                Title = title,
                Description = description,
                Quantity = quantity,
                ImagePath = string.Empty,
                Categories = categories
            };
            var imageStream = new MemoryStream();
            var expectedPublisherData = new InventoryUpdatedMessage
            {
                Barcode = barcode,
                UpdatedInventory = updatedInventory,
                MessageId = message.MessageId,
                Timestamp = message.Timestamp
            };
            var expectedPublisherResponse = PublisherResponse.Failed(["error publishing message"], [expectedPublisherData]);

            mockInventoryRepository.Setup(x => x.Get(barcode)).ReturnsAsync(inventory);
            mockBarcodeLookup.Setup(x => x.Get(barcode)).ReturnsAsync(new Barcode
            {
                product = new BarcodeProduct
                {
                    barcode = inventory.Barcode,
                    title = title,
                    description = description,
                    images = []
                }
            });
            mockInventoryRepository.Setup(x => x.Insert(updatedInventory)).ReturnsAsync(1);
            mockInventoryUpdatedPublisher.Setup(x => x.Publish(updatedInventory)).ReturnsAsync(expectedPublisherResponse);

            Assert.DoesNotThrowAsync(async () => await handler.Handle(message));

            mockInventoryRepository.Verify(x => x.Get(barcode), Times.Once);
            mockBarcodeLookup.Verify(x => x.Get(inventory.Barcode), Times.Once);
            mockImageLookup.Verify(x => x.Get(It.IsAny<string>()), Times.Never);
            mockImageRepository.Verify(x => x.Insert(It.IsAny<MemoryStream>(), It.IsAny<string>()), Times.Never);
            mockInventoryRepository.Verify(x => x.Insert(
                It.Is<Inventory>(x =>
                    x.Barcode == barcode &&
                    x.Title == title &&
                    x.Description == description &&
                    x.Quantity == quantity &&
                    x.ImagePath == imagePath &&
                    x.Categories.SequenceEqual(categories)
                )), Times.Once);
        }

        [Test]
        public void When_calling_handle_and_barcode_lookup_returns_multiple_images()
        {
            var barcode = Barcodes.Generate();
            var title = "Test-Product";
            var description = "Test-Description";
            var quantity = 1;
            var categories = new List<string>();
            var imageUrl1 = "https://test.com/image1.jpg";
            var imageUrl2 = "https://test.com/image2.jpg";
            var imagePath = Directory.GetCurrentDirectory() + $"/Images/{title}-{barcode}.jpg";
            var inventory = new Inventory
            {
                Barcode = barcode,
                Quantity = quantity
            };
            var message = new FetchInventoryMetadataMessage
            {
                Barcode = barcode,
                MessageId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow
            };

            var updatedInventory = new Inventory
            {
                Barcode = barcode,
                Title = title,
                Description = description,
                Quantity = quantity,
                ImagePath = imagePath,
                Categories = categories
            };
            var imageStream = new MemoryStream();
            var expectedPublisherData = new InventoryUpdatedMessage
            {
                Barcode = barcode,
                UpdatedInventory = updatedInventory,
                MessageId = message.MessageId,
                Timestamp = message.Timestamp
            };
            var expectedPublisherResponse = PublisherResponse.Failed(["error publishing message"], [expectedPublisherData]);

            mockInventoryRepository.Setup(x => x.Get(barcode)).ReturnsAsync(inventory);
            mockBarcodeLookup.Setup(x => x.Get(barcode)).ReturnsAsync(new Barcode
            {
                product = new BarcodeProduct
                {
                    barcode = inventory.Barcode,
                    title = title,
                    description = description,
                    images = [imageUrl1, imageUrl2]
                }
            });
            mockImageLookup.Setup(x => x.Get(imageUrl1)).ReturnsAsync(imageStream);
            mockImageRepository.Setup(x => x.Insert(imageStream, imagePath)).ReturnsAsync("success");
            mockInventoryRepository.Setup(x => x.Insert(updatedInventory)).ReturnsAsync(1);
            mockInventoryUpdatedPublisher.Setup(x => x.Publish(updatedInventory)).ReturnsAsync(expectedPublisherResponse);

            Assert.DoesNotThrowAsync(async () => await handler.Handle(message));

            mockInventoryRepository.Verify(x => x.Get(barcode), Times.Once);
            mockBarcodeLookup.Verify(x => x.Get(inventory.Barcode), Times.Once);
            mockImageLookup.Verify(x => x.Get(imageUrl1), Times.Once);
            mockImageRepository.Verify(x => x.Insert(imageStream, imagePath), Times.Once);
            mockInventoryRepository.Verify(x => x.Insert(
                It.Is<Inventory>(x =>
                    x.Barcode == barcode &&
                    x.Title == title &&
                    x.Description == description &&
                    x.Quantity == quantity &&
                    x.ImagePath == imagePath &&
                    x.Categories.SequenceEqual(categories)
                )), Times.Once);
        }

        [Test]
        public void When_calling_add_workflow_and_the_image_stream_cannot_be_retrieved()
        {
            var barcode = Barcodes.Generate();
            var title = "Test-Product";
            var description = "Test-Description";
            var quantity = 1;
            var categories = new List<string>();
            var imageUrl = "https://test.com/image.jpg";
            var imagePath = "";
            var inventory = new Inventory
            {
                Barcode = barcode,
                Quantity = quantity
            };
            var message = new FetchInventoryMetadataMessage
            {
                Barcode = barcode,
                MessageId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow
            };
            var imageStream = null as Stream;
            var updatedInventory = new Inventory
            {
                Barcode = barcode,
                Title = title,
                Description = description,
                Quantity = quantity,
                ImagePath = imagePath,
                Categories = categories
            };
            var expectedPublisherData = new InventoryUpdatedMessage
            {
                Barcode = barcode,
                UpdatedInventory = updatedInventory,
                MessageId = message.MessageId,
                Timestamp = message.Timestamp
            };
            var expectedPublisherResponse = PublisherResponse.Failed(["error publishing message"], [expectedPublisherData]);

            mockInventoryRepository.Setup(x => x.Get(barcode)).ReturnsAsync(inventory);
            mockBarcodeLookup.Setup(x => x.Get(barcode)).ReturnsAsync(new Barcode
            {
                product = new BarcodeProduct
                {
                    barcode = inventory.Barcode,
                    title = title,
                    description = description,
                    images = [imageUrl]
                }
            });
            mockImageLookup.Setup(x => x.Get(imageUrl)).ReturnsAsync(imageStream);
            mockInventoryRepository.Setup(x => x.Insert(updatedInventory)).ReturnsAsync(1);
            mockInventoryUpdatedPublisher.Setup(x => x.Publish(updatedInventory)).ReturnsAsync(expectedPublisherResponse);
            mockLogger.Setup(x => x.Warning(It.IsAny<LogContext>()));

            Assert.DoesNotThrowAsync(async () => await handler.Handle(message));

            mockInventoryRepository.Verify(x => x.Get(barcode), Times.Once);
            mockBarcodeLookup.Verify(x => x.Get(inventory.Barcode), Times.Once);
            mockImageLookup.Verify(x => x.Get(imageUrl), Times.Once);
            mockImageRepository.Verify(x => x.Insert(It.IsAny<Stream>(), It.IsAny<string>()), Times.Never);
            mockInventoryRepository.Verify(x => x.Insert(
                It.Is<Inventory>(x =>
                    x.Barcode == barcode &&
                    x.Title == title &&
                    x.Description == description &&
                    x.Quantity == quantity &&
                    x.ImagePath == imagePath &&
                    x.Categories.SequenceEqual(categories)
                )), Times.Once);
            mockLogger.Verify(
                x => x.Warning(
                    It.Is<LogContext>(l =>
                        l.Barcode == barcode &&
                        l.Message == "Error looking up barcode: Image retrieval failed." &&
                        l.Component == typeof(FetchInventoryMetadataMessageHandler).Name &&
                        l.Operation == "Save Image")
                ),
                Times.Once);
        }

        [Test]
        public void When_calling_handle_and_the_image_cannot_be_saved()
        {
            var barcode = Barcodes.Generate();
            var title = "Test-Product";
            var description = "Test-Description";
            var quantity = 1;
            var categories = new List<string>();
            var imageUrl = "https://test.com/image.jpg";
            var imagePath = Directory.GetCurrentDirectory() + $"/Images/{title}-{barcode}.jpg";
            var imageRepoErrorMessage = "Error Message";
            var inventory = new Inventory
            {
                Barcode = barcode,
                Quantity = quantity
            };
            var message = new FetchInventoryMetadataMessage
            {
                Barcode = barcode,
                MessageId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow
            };
            var imageStream = new MemoryStream();
            var updatedInventory = new Inventory
            {
                Barcode = barcode,
                Title = title,
                Description = description,
                Quantity = quantity,
                ImagePath = string.Empty,
                Categories = categories
            };
            var expectedPublisherData = new InventoryUpdatedMessage
            {
                Barcode = barcode,
                UpdatedInventory = updatedInventory,
                MessageId = message.MessageId,
                Timestamp = message.Timestamp
            };
            var expectedPublisherResponse = PublisherResponse.Failed(["error publishing message"], [expectedPublisherData]);

            mockInventoryRepository.Setup(x => x.Get(barcode)).ReturnsAsync(inventory);
            mockBarcodeLookup.Setup(x => x.Get(barcode)).ReturnsAsync(new Barcode
            {
                product = new BarcodeProduct
                {
                    barcode = inventory.Barcode,
                    title = title,
                    description = description,
                    images = [imageUrl]
                }
            });
            mockImageLookup.Setup(x => x.Get(imageUrl)).ReturnsAsync(imageStream);
            mockImageRepository.Setup(x => x.Insert(imageStream, imagePath)).ReturnsAsync(imageRepoErrorMessage);
            mockInventoryRepository.Setup(x => x.Insert(updatedInventory)).ReturnsAsync(1);
            mockInventoryUpdatedPublisher.Setup(x => x.Publish(updatedInventory)).ReturnsAsync(expectedPublisherResponse);
            mockLogger.Setup(x => x.Warning(It.IsAny<LogContext>()));

            Assert.DoesNotThrowAsync(async () => await handler.Handle(message));

            mockInventoryRepository.Verify(x => x.Get(barcode), Times.Once);
            mockBarcodeLookup.Verify(x => x.Get(inventory.Barcode), Times.Once);
            mockImageLookup.Verify(x => x.Get(imageUrl), Times.Once);
            mockImageRepository.Verify(x => x.Insert(imageStream, imagePath), Times.Once);
            mockInventoryRepository.Verify(x => x.Insert(
                It.Is<Inventory>(x =>
                    x.Barcode == barcode &&
                    x.Title == title &&
                    x.Description == description &&
                    x.Quantity == quantity &&
                    x.ImagePath == string.Empty &&
                    x.Categories.SequenceEqual(categories)
                )), Times.Once);
            mockLogger.Verify(
                x => x.Warning(
                    It.Is<LogContext>(l =>
                        l.Barcode == barcode &&
                        l.Message == "Error looking up barcode: Failed to save image." &&
                        l.Component == typeof(FetchInventoryMetadataMessageHandler).Name &&
                        l.Operation == "Save Image")
                ),
                Times.Once);
        }

        [Test]
        public void When_calling_handle_and_the_inventory_fails_to_save()
        {
            var barcode = Barcodes.Generate();
            var title = "Test-Product";
            var description = "Test-Description";
            var quantity = 1;
            var categories = new List<string>();
            var imageUrl = "https://test.com/image.jpg";
            var imagePath = Directory.GetCurrentDirectory() + $"/Images/{title}-{barcode}.jpg";
            var inventory = new Inventory
            {
                Barcode = barcode,
                Quantity = quantity
            };
            var message = new FetchInventoryMetadataMessage
            {
                Barcode = barcode,
                MessageId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow
            };
            var updatedInventory = new Inventory
            {
                Barcode = barcode,
                Title = title,
                Description = description,
                Quantity = quantity,
                ImagePath = imagePath,
                Categories = categories
            };
            var imageStream = new MemoryStream();

            mockInventoryRepository.Setup(x => x.Get(barcode)).ReturnsAsync(inventory);
            mockBarcodeLookup.Setup(x => x.Get(barcode)).ReturnsAsync(new Barcode
            {
                product = new BarcodeProduct
                {
                    barcode = inventory.Barcode,
                    title = title,
                    description = description,
                    images = [imageUrl]
                }
            });
            mockImageLookup.Setup(x => x.Get(imageUrl)).ReturnsAsync(imageStream);
            mockImageRepository.Setup(x => x.Insert(imageStream, imagePath)).ReturnsAsync("success");
            mockInventoryRepository.Setup(x => x.Insert(updatedInventory)).ReturnsAsync(0);

            var exception = Assert.ThrowsAsync<Exception>(async () => await handler.Handle(message));

            Assert.That(exception.Message, Is.EqualTo("Error handling metadata update message: Failed to save inventory."));

            mockInventoryRepository.Verify(x => x.Get(barcode), Times.Once);
            mockBarcodeLookup.Verify(x => x.Get(inventory.Barcode), Times.Once);
            mockImageLookup.Verify(x => x.Get(imageUrl), Times.Once);
            mockImageRepository.Verify(x => x.Insert(imageStream, imagePath), Times.Once);
            mockInventoryRepository.Verify(x => x.Insert(
                It.Is<Inventory>(x =>
                    x.Barcode == barcode &&
                    x.Title == title &&
                    x.Description == description &&
                    x.Quantity == quantity &&
                    x.ImagePath == imagePath &&
                    x.Categories.SequenceEqual(categories)
                )), Times.Once);

        }
    }
}
