using InventoryScanner.Core.Messages;
using InventoryScanner.Core.Models;
using InventoryScanner.Core.Publishers;
using InventoryScanner.Core.Repositories;
using InventoryScanner.Core.Workflows;
using InventoryScanner.Messaging.Enums;
using InventoryScanner.Messaging.Publishing;
using InventoryScanner.TestUtilities;
using Moq;

namespace InventoryScanner.Core.UnitTests
{
    [TestFixture]
    public class InventoryWorkflowTests
    {
        InventoryWorkflow workflow;
        Mock<IInventoryRepository> mockInventoryRepository;
        Mock<IFetchInventoryMetadataRequestPublisher> mockFetchInventoryMetadataRequestPublisher;

        [SetUp]
        public void Setup()
        {
            mockInventoryRepository = new Mock<IInventoryRepository>();
            mockFetchInventoryMetadataRequestPublisher = new Mock<IFetchInventoryMetadataRequestPublisher>();
            workflow = new InventoryWorkflow(mockInventoryRepository.Object, mockFetchInventoryMetadataRequestPublisher.Object);
        }

        [Test]
        public async Task When_calling_get_workflow_successfully()
        {
            var barcode = Barcodes.Generate();
            var title = "Test-Product";
            var description = "Test-Description";
            var quantity = 1;
            var categories = new List<string>();
            var imagePath = Directory.GetCurrentDirectory() + $"/Images/{title}-{barcode}.png";
            var expectedInventory = new Inventory
            {
                Barcode = barcode,
                Title = title,
                Description = description,
                Quantity = quantity,
                ImagePath = imagePath,
                Categories = categories
            };
            var expectedResponse = InventoryWorkflowResponse.Success([expectedInventory]);

            mockInventoryRepository.Setup(x => x.Get(barcode)).ReturnsAsync(expectedInventory);

            var result = await workflow.Get(barcode);

            Assert.That(result, Is.EqualTo(expectedResponse));

            mockInventoryRepository.Verify(x => x.Get(barcode), Times.Once);
        }

        [Test]
        public async Task When_calling_get_workflow_and_retrieval_throws()
        {
            var barcode = "123456";
            var expectedResponse = InventoryWorkflowResponse.Failure("Error looking up barcode: Failed to retrieve inventory.");

            mockInventoryRepository.Setup(x => x.Get(barcode)).ThrowsAsync(new Exception("Unknown error"));

            var result = await workflow.Get(barcode);

            Assert.That(result, Is.EqualTo(expectedResponse));

            mockInventoryRepository.Verify(x => x.Get(barcode), Times.Once);
        }

        [Test]
        public async Task When_calling_get_workflow_and_retrieval_is_empty()
        {
            var barcode = "123456";
            var expectedResponse = InventoryWorkflowResponse.Failure("Error looking up barcode: Inventory not found.");

            mockInventoryRepository.Setup(x => x.Get(barcode)).ReturnsAsync(null as Inventory);

            var result = await workflow.Get(barcode);

            Assert.That(result, Is.EqualTo(expectedResponse));

            mockInventoryRepository.Verify(x => x.Get(barcode), Times.Once);
        }

        [Test]
        public async Task When_calling_get_all_workflow_successfully()
        {
            var inventoryList = new List<Inventory>
            {
                new Inventory
                {
                    Barcode = "123456",
                    Title = "Test-Product",
                    Description = "Test-Description",
                    Quantity = 1,
                    ImagePath = Directory.GetCurrentDirectory() + $"/Images/Test-Product-123456.png",
                    Categories = new List<string>()
                },
                new Inventory
                {
                    Barcode = "654321",
                    Title = "Test-Product-2",
                    Description = "Test-Description-2",
                    Quantity = 2,
                    ImagePath = Directory.GetCurrentDirectory() + $"/Images/Test-Product-2-654321.png",
                    Categories = new List<string>()
                }
            };
            var expectedResponse = InventoryWorkflowResponse.Success(inventoryList);

            mockInventoryRepository.Setup(x => x.GetAll()).ReturnsAsync(inventoryList);

            var result = await workflow.GetAll();

            Assert.That(result, Is.EqualTo(expectedResponse));
        }

        [Test]
        public async Task When_calling_get_all_workflow_and_retrieval_throws()
        {
            var expectedResponse = InventoryWorkflowResponse.Failure("Error looking up all inventory: Failed to retrieve inventory.");

            mockInventoryRepository.Setup(x => x.GetAll()).ThrowsAsync(new Exception("Unknown error"));

            var result = await workflow.GetAll();

            Assert.That(result, Is.EqualTo(expectedResponse));

            mockInventoryRepository.Verify(x => x.GetAll(), Times.Once);
        }

        [Test]
        public async Task When_calling_get_all_workflow_and_retrieval_is_empty()
        {
            var expectedResponse = InventoryWorkflowResponse.Failure("Error looking up all inventory: Inventory not found.");

            mockInventoryRepository.Setup(x => x.GetAll()).ReturnsAsync([]);

            var result = await workflow.GetAll();

            Assert.That(result, Is.EqualTo(expectedResponse));

            mockInventoryRepository.Verify(x => x.GetAll(), Times.Once);
        }

        [Test]
        public async Task When_calling_add_workflow_successfully()
        {
            var barcode = Barcodes.Generate();
            var messageId = Guid.NewGuid();
            var timestamp = DateTime.UtcNow;
            var quantity = 1;
            var inventory = new Inventory
            {
                Barcode = barcode,
                Quantity = quantity
            };
            var expectedPublisherResponse = new PublisherResponse
            (
                PublisherResponseStatus.Success,
                [new FetchInventoryMetadataMessage { Barcode = barcode, MessageId = messageId, Timestamp = timestamp }],
                []
            );
            var expectedWorkflowResponse = InventoryWorkflowResponse.Success([inventory]);

            mockFetchInventoryMetadataRequestPublisher.Setup(x => x.PublishRequest(barcode)).ReturnsAsync(expectedPublisherResponse);
            mockInventoryRepository.Setup(x => x.Insert(inventory)).ReturnsAsync(1);

            var result = await workflow.Add(inventory);

            Assert.That(result, Is.EqualTo(expectedWorkflowResponse));

            mockFetchInventoryMetadataRequestPublisher.Verify(x => x.PublishRequest(barcode), Times.Once);
            mockInventoryRepository.Verify(x => x.Insert(
                It.Is<Inventory>(x =>
                    x.Barcode == barcode &&
                    x.Quantity == quantity
                )), Times.Once);
        }

        [Test]
        public async Task When_calling_add_workflow_and_the_publish_fails()
        {
            var barcode = Barcodes.Generate();
            var messageId = Guid.NewGuid();
            var timestamp = DateTime.UtcNow;
            var quantity = 1;
            var inventory = new Inventory
            {
                Barcode = barcode,
                Quantity = quantity
            };
            var publishError = "error publishing";
            var expectedPublisherResponse = new PublisherResponse
            (
                PublisherResponseStatus.Failure,
                [],
                [publishError]
            );
            var expectedWorkflowResponse = InventoryWorkflowResponse.Success([inventory]);
            expectedWorkflowResponse.Errors.Add(publishError);  

            mockFetchInventoryMetadataRequestPublisher.Setup(x => x.PublishRequest(barcode)).ReturnsAsync(expectedPublisherResponse);
            mockInventoryRepository.Setup(x => x.Insert(inventory)).ReturnsAsync(1);

            var result = await workflow.Add(inventory);

            Assert.That(result, Is.EqualTo(expectedWorkflowResponse));

            mockFetchInventoryMetadataRequestPublisher.Verify(x => x.PublishRequest(barcode), Times.Once);
            mockInventoryRepository.Verify(x => x.Insert(
                It.Is<Inventory>(x =>
                    x.Barcode == barcode &&
                    x.Quantity == quantity
                )), Times.Once);
        }

        [Test]
        public async Task When_calling_add_workflow_and_the_repository_fails_to_save()
        {
            var barcode = Barcodes.Generate();
            var messageId = Guid.NewGuid();
            var timestamp = DateTime.UtcNow;
            var quantity = 1;
            var inventory = new Inventory
            {
                Barcode = barcode,
                Quantity = quantity
            };
            var repositoryError = $"Error saving barcode {barcode}: Failed to update inventory.";
            var expectedWorkflowResponse = InventoryWorkflowResponse.Failure(repositoryError);

            mockInventoryRepository.Setup(x => x.Insert(inventory)).ReturnsAsync(0);

            var result = await workflow.Add(inventory);

            Assert.That(result, Is.EqualTo(expectedWorkflowResponse));

            mockFetchInventoryMetadataRequestPublisher.Verify(x => x.PublishRequest(It.IsAny<string>()), Times.Never);
            mockInventoryRepository.Verify(x => x.Insert(
                It.Is<Inventory>(x =>
                    x.Barcode == barcode &&
                    x.Quantity == quantity
                )), Times.Once);
        }

        [Test]
        public async Task When_calling_update_workflow_sucessfully_with_refetch_false()
        {
            var barcode = Barcodes.Generate();
            var title = "Test-Product";
            var description = "Test-Description";
            var quantity = 1;
            var categories = new List<string>();
            var imagePath = Directory.GetCurrentDirectory() + $"/Images/{title}-{barcode}.png";
            var inventory = new Inventory { Barcode = barcode, Quantity = quantity + 1 };
            var updatedInventory = new Inventory
            {
                Barcode = barcode,
                Title = title,
                Description = description,
                Quantity = quantity + 1,
                ImagePath = imagePath,
                Categories = categories
            };
            var expectedResponse = InventoryWorkflowResponse.Success([updatedInventory]);

            mockInventoryRepository.Setup(x => x.Get(barcode)).ReturnsAsync(updatedInventory);
            mockInventoryRepository.Setup(x => x.Insert(inventory)).ReturnsAsync(1);

            var result = await workflow.Update(inventory, refetch: false);

            Assert.That(result, Is.EqualTo(expectedResponse));

            mockInventoryRepository.Verify(x => x.Get(barcode), Times.Once);
            mockInventoryRepository.Verify(x => x.Insert(inventory), Times.Once);
            mockFetchInventoryMetadataRequestPublisher.Verify(x => x.PublishRequest(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task When_calling_update_workflow_sucessfully_with_refetch_true()
        {
            var barcode = Barcodes.Generate();
            var title = "Test-Product";
            var description = "Test-Description";
            var quantity = 1;
            var categories = new List<string>();
            var imagePath = Directory.GetCurrentDirectory() + $"/Images/{title}-{barcode}.png";
            var inventory = new Inventory { Barcode = barcode, Quantity = quantity + 1 };
            var messageId = Guid.NewGuid();
            var timestamp = DateTime.UtcNow;
            var expectedPublisherResponse = new PublisherResponse
            (
                PublisherResponseStatus.Success,
                [new FetchInventoryMetadataMessage { Barcode = barcode, MessageId = messageId, Timestamp = timestamp }],
                []
            );
            var updatedInventory = new Inventory
            {
                Barcode = barcode,
                Title = title,
                Description = description,
                Quantity = quantity + 1,
                ImagePath = imagePath,
                Categories = categories
            };
            var expectedResponse = InventoryWorkflowResponse.Success([updatedInventory]);

            mockFetchInventoryMetadataRequestPublisher.Setup(x => x.PublishRequest(barcode)).ReturnsAsync(expectedPublisherResponse);
            mockInventoryRepository.Setup(x => x.Get(barcode)).ReturnsAsync(updatedInventory);
            mockInventoryRepository.Setup(x => x.Insert(inventory)).ReturnsAsync(1);

            var result = await workflow.Update(inventory, refetch: true);

            Assert.That(result, Is.EqualTo(expectedResponse));

            mockInventoryRepository.Verify(x => x.Get(barcode), Times.Once);
            mockInventoryRepository.Verify(x => x.Insert(inventory), Times.Once);
            mockFetchInventoryMetadataRequestPublisher.Verify(x => x.PublishRequest(barcode), Times.Once);
        }

        [Test]
        public async Task When_calling_update_workflow_and_the_publish_fails()
        {
            var barcode = Barcodes.Generate();
            var messageId = Guid.NewGuid();
            var timestamp = DateTime.UtcNow;
            var quantity = 1;
            var title = "Test-Product";
            var description = "Test-Description";
            var categories = new List<string>();
            var imagePath = Directory.GetCurrentDirectory() + $"/Images/{title}-{barcode}.png";
            var inventory = new Inventory
            {
                Barcode = barcode,
                Quantity = quantity + 1
            };
            var updatedInventory = new Inventory
            {
                Barcode = barcode,
                Title = title,
                Description = description,
                Quantity = quantity + 1,
                ImagePath = imagePath,
                Categories = categories
            };
            var publishError = "error publishing";
            var expectedPublisherResponse = new PublisherResponse
            (
                PublisherResponseStatus.Failure,
                [],
                [publishError]
            );
            var expectedWorkflowResponse = InventoryWorkflowResponse.Success([updatedInventory]);
            expectedWorkflowResponse.Errors.Add(publishError);

            mockFetchInventoryMetadataRequestPublisher.Setup(x => x.PublishRequest(barcode)).ReturnsAsync(expectedPublisherResponse);
            mockInventoryRepository.Setup(x => x.Get(barcode)).ReturnsAsync(updatedInventory);
            mockInventoryRepository.Setup(x => x.Insert(inventory)).ReturnsAsync(1);

            var result = await workflow.Update(inventory, refetch: true);

            Assert.That(result, Is.EqualTo(expectedWorkflowResponse));

            mockFetchInventoryMetadataRequestPublisher.Verify(x => x.PublishRequest(barcode), Times.Once);
            mockInventoryRepository.Verify(x => x.Insert(
                It.Is<Inventory>(x =>
                    x.Barcode == barcode &&
                    x.Quantity == quantity + 1
                )), Times.Once);
        }

        [Test]
        public async Task When_calling_update_workflow_and_the_repository_fails_to_save()
        {
            var barcode = Barcodes.Generate();
            var quantity = 1;
            var inventory = new Inventory
            {
                Barcode = barcode,
                Quantity = quantity
            };
            var repositoryError = $"Error updating barcode {barcode}: Failed to update inventory.";
            
            var expectedWorkflowResponse = InventoryWorkflowResponse.Failure(repositoryError);

            mockInventoryRepository.Setup(x => x.Insert(inventory)).ReturnsAsync(0);

            var result = await workflow.Update(inventory);

            Assert.That(result, Is.EqualTo(expectedWorkflowResponse));

            mockInventoryRepository.Verify(x => x.Insert(
                It.Is<Inventory>(x =>
                    x.Barcode == barcode &&
                    x.Quantity == quantity
                )), Times.Once);
        }
    }
}
