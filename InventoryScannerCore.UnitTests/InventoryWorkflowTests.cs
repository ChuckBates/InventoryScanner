using InventoryScannerCore.Lookups;
using InventoryScannerCore.Models;
using InventoryScannerCore.Repositories;
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
        public async Task When_calling_add_workflow()
        {
            var barcode = "123456";
            var title = "Test Product";
            var description = "Test Description";
            var quantity = 1;
            var categories = Array.Empty<string>();
            var imageUrl = "https://test.com/image.jpg";
            var imagePath = $"/Images/{title}-{barcode}.jpeg";
            var inventory = new Inventory
            {
                Barcode = barcode,
                Quantity = quantity
            };
            var imageStream = new MemoryStream();

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
            mockImageRepository.Setup(x => x.Insert(imageStream, imagePath)).ReturnsAsync(imagePath);
            mockInventoryRepository.Setup(x => x.Insert(inventory));

            var result = await workflow.Add(inventory);

            Assert.That(result, Is.EqualTo("success"));
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
                    x.Categories == categories
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

            mockBarcodeLookup.Setup(x => x.Get(barcode)).ReturnsAsync(null as Barcode);

            var result = await workflow.Add(inventory);

            Assert.That(result, Is.EqualTo("Barcode not found"));

            mockBarcodeLookup.Verify(x => x.Get(inventory.Barcode), Times.Once);
            mockImageLookup.Verify(x => x.Get(It.IsAny<string>()), Times.Never);
            mockImageRepository.Verify(x => x.Insert(It.IsAny<Stream>(), It.IsAny<string>()), Times.Never);
            mockInventoryRepository.Verify(x => x.Insert(It.IsAny<Inventory>()), Times.Never);
        }

        [Test]
        public async Task When_calling_add_workflow_and_barcode_lookup_returns_multiple_products()
        {

        }

        [Test]
        public async Task When_calling_add_workflow_and_barcode_lookup_returns_no_images()
        {

        }

        [Test]
        public async Task When_calling_add_workflow_and_barcode_lookup_returns_multiple_images()
        {

        }

        [Test]
        public async Task When_calling_add_workflow_and_no_image_can_be_found()
        {

        }

        [Test]
        public async Task When_calling_add_workflow_and_the_image_cannot_be_saved()
        {

        }

        [Test]
        public async Task When_calling_add_workflow_and_the_inventory_fails_to_save()
        {

        }
    }
}
