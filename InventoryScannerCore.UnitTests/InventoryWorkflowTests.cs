using InventoryScannerCore.Enums;
using InventoryScannerCore.Lookups;
using InventoryScannerCore.Models;
using InventoryScannerCore.Repositories;
using InventoryScannerCore.Workflows;
using Moq;

namespace InventoryScannerCore.UnitTests
{
    [TestFixture]
    public class InventoryWorkflowTests
    {
        InventoryWorkflow workflow;
        Mock<IInventoryRepository> mockInventoryRepository;
        Mock<IBarcodeLookup> mockBarcodeLookup;
        Mock<IImageRepository> mockImageRepository;
        Mock<IImageLookup> mockImageLookup;

        [SetUp]
        public void Setup()
        {
            mockInventoryRepository = new Mock<IInventoryRepository>();
            mockBarcodeLookup = new Mock<IBarcodeLookup>();
            mockImageRepository = new Mock<IImageRepository>();
            mockImageLookup = new Mock<IImageLookup>();
            workflow = new InventoryWorkflow(mockInventoryRepository.Object, mockBarcodeLookup.Object, mockImageLookup.Object, mockImageRepository.Object);
        }

        [Test]
        public async Task When_calling_get_workflow()
        {
            var barcode = "123456";
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
            var expectedResponse = new InventoryWorkflowResponse(WorkflowResponseStatus.Success, [expectedInventory], []);

            mockInventoryRepository.Setup(x => x.Get(barcode)).ReturnsAsync(expectedInventory);

            var result = await workflow.Get(barcode);

            Assert.That(result, Is.EqualTo(expectedResponse));

            mockInventoryRepository.Verify(x => x.Get(barcode), Times.Once);
        }

        [Test]
        public async Task When_calling_get_workflow_and_retrieval_throws()
        {
            var barcode = "123456";
            var expectedResponse = new InventoryWorkflowResponse(WorkflowResponseStatus.Error, [], ["Error looking up barcode: Failed to retrieve inventory."]);

            mockInventoryRepository.Setup(x => x.Get(barcode)).ThrowsAsync(new Exception("Unknown error"));

            var result = await workflow.Get(barcode);

            Assert.That(result, Is.EqualTo(expectedResponse));

            mockInventoryRepository.Verify(x => x.Get(barcode), Times.Once);
        }

        [Test]
        public async Task When_calling_get_workflow_and_retrieval_is_empty()
        {
            var barcode = "123456";
            var expectedResponse = new InventoryWorkflowResponse(WorkflowResponseStatus.Error, [], ["Error looking up barcode: Inventory not found."]);

            mockInventoryRepository.Setup(x => x.Get(barcode)).ReturnsAsync(null as Inventory);

            var result = await workflow.Get(barcode);

            Assert.That(result, Is.EqualTo(expectedResponse));

            mockInventoryRepository.Verify(x => x.Get(barcode), Times.Once);
        }

        [Test]
        public async Task When_calling_get_all_workflow()
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
            var expectedResponse = new InventoryWorkflowResponse(WorkflowResponseStatus.Success, inventoryList, []);

            mockInventoryRepository.Setup(x => x.GetAll()).ReturnsAsync(inventoryList);

            var result = await workflow.GetAll();

            Assert.That(result, Is.EqualTo(expectedResponse));
        }

        [Test]
        public async Task When_calling_get_all_workflow_and_retrieval_throws()
        {
            var expectedResponse = new InventoryWorkflowResponse(WorkflowResponseStatus.Error, [], ["Error looking up all inventory: Failed to retrieve inventory."]);

            mockInventoryRepository.Setup(x => x.GetAll()).ThrowsAsync(new Exception("Unknown error"));

            var result = await workflow.GetAll();

            Assert.That(result, Is.EqualTo(expectedResponse));

            mockInventoryRepository.Verify(x => x.GetAll(), Times.Once);
        }

        [Test]
        public async Task When_calling_get_all_workflow_and_retrieval_is_empty()
        {
            var expectedResponse = new InventoryWorkflowResponse(WorkflowResponseStatus.Error, [], ["Error looking up all inventory: Inventory not found."]);

            mockInventoryRepository.Setup(x => x.GetAll()).ReturnsAsync([]);

            var result = await workflow.GetAll();

            Assert.That(result, Is.EqualTo(expectedResponse));

            mockInventoryRepository.Verify(x => x.GetAll(), Times.Once);
        }

        [Test]
        public async Task When_calling_add_workflow()
        {
            var barcode = "123456";
            var title = "Test-Product";
            var description = "Test-Description";
            var quantity = 1;
            var categories = new List<string>();
            var imageUrl = "https://test.com/image.png";
            var imagePath = Directory.GetCurrentDirectory() + $"/Images/{title}-{barcode}.png";
            var inventory = new Inventory
            {
                Barcode = barcode,
                Quantity = quantity
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
            var expectedResponse = new InventoryWorkflowResponse(WorkflowResponseStatus.Success, [updatedInventory], []);

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
            mockInventoryRepository.Setup(x => x.Insert(updatedInventory)).ReturnsAsync(1);

            var result = await workflow.Add(inventory);

            Assert.That(result, Is.EqualTo(expectedResponse));

            mockBarcodeLookup.Verify(x => x.Get(inventory.Barcode), Times.Once);
            mockImageLookup.Verify(x => x.Get(imageUrl), Times.Once);
            mockImageRepository.Verify(x => x.Insert(imageStream, imagePath), Times.Once);
            mockInventoryRepository.Verify(x => x.Insert(
                It.Is<Inventory>(x => 
                    x.Barcode == barcode &&
                    x.Title == title &&
                    x.Description == description &&
                    x.Quantity ==  quantity &&
                    x.ImagePath == imagePath &&
                    x.Categories.SequenceEqual(categories)
                )), Times.Once);
        }

        [Test]
        public async Task When_calling_add_workflow_and_barcode_lookup_returns_null()
        {
            var barcode = "123456";
            var inventory = new Inventory
            {
                Barcode = barcode
            };
            var expectedResponse = new InventoryWorkflowResponse(WorkflowResponseStatus.Error, [], ["Error looking up barcode: Barcode not found."]);

            mockBarcodeLookup.Setup(x => x.Get(barcode)).ReturnsAsync(null as Barcode);

            var result = await workflow.Add(inventory);

            Assert.That(result, Is.EqualTo(expectedResponse));

            mockBarcodeLookup.Verify(x => x.Get(inventory.Barcode), Times.Once);
            mockImageLookup.Verify(x => x.Get(It.IsAny<string>()), Times.Never);
            mockImageRepository.Verify(x => x.Insert(It.IsAny<Stream>(), It.IsAny<string>()), Times.Never);
            mockInventoryRepository.Verify(x => x.Insert(It.IsAny<Inventory>()), Times.Never);
        }

        [Test]
        public async Task When_calling_add_workflow_and_barcode_lookup_returns_no_images()
        {
            var barcode = "123456";
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
            var expectedResponse = new InventoryWorkflowResponse(WorkflowResponseStatus.Success, [updatedInventory], ["Error looking up barcode: Image not found."]);

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
            mockImageRepository.Setup(x => x.Insert(imageStream, imagePath)).ReturnsAsync("success");
            mockInventoryRepository.Setup(x => x.Insert(updatedInventory)).ReturnsAsync(1);

            var result = await workflow.Add(inventory);

            Assert.That(result, Is.EqualTo(expectedResponse));

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
        public async Task When_calling_add_workflow_and_barcode_lookup_returns_multiple_images()
        {
            var barcode = "123456";
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
            var expectedResponse = new InventoryWorkflowResponse(WorkflowResponseStatus.Success, [updatedInventory], []);

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

            var result = await workflow.Add(inventory);

            Assert.That(result, Is.EqualTo(expectedResponse));

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
        public async Task When_calling_add_workflow_and_the_image_stream_cannot_be_retrieved()
        {
            var barcode = "123456";
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
            var expectedResponse = new InventoryWorkflowResponse(WorkflowResponseStatus.Success, [updatedInventory], ["Error looking up barcode: Image retrieval failed."]);

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

            var result = await workflow.Add(inventory);

            Assert.That(result, Is.EqualTo(expectedResponse));

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
        }

        [Test]
        public async Task When_calling_add_workflow_and_the_image_cannot_be_saved()
        {
            var barcode = "123456";
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
            var expectedResponse = new InventoryWorkflowResponse(WorkflowResponseStatus.Success, [updatedInventory], ["Error looking up barcode: Failed to save image."]);

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

            var result = await workflow.Add(inventory);

            Assert.That(result, Is.EqualTo(expectedResponse));

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
        }

        [Test]
        public async Task When_calling_add_workflow_and_the_inventory_fails_to_save()
        {
            var barcode = "123456";
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
            var expectedResponse = new InventoryWorkflowResponse(WorkflowResponseStatus.Error, [], ["Error looking up barcode: Failed to save inventory."]);

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

            var result = await workflow.Add(inventory);

            Assert.That(result, Is.EqualTo(expectedResponse));

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
