using InventoryScannerCore.Controllers;
using InventoryScannerCore.Enums;
using InventoryScannerCore.Models;
using InventoryScannerCore.Repositories;
using Moq;

namespace InventoryScannerCore.UnitTests
{
    [TestFixture]
    public class InventoryControllerTests
    {
        InventoryController inventoryController;
        Mock<IInventoryRepository> mockInventoryRepository;

        [SetUp]
        public void Setup()
        {
            mockInventoryRepository = new Mock<IInventoryRepository>();
            inventoryController = new InventoryController(mockInventoryRepository.Object);
        }

        [Test]
        public async Task When_calling_get_all_inventory_and_there_is_an_errorAsync()
        {
            var error = "An error occurred.";
            mockInventoryRepository.Setup(x => x.GetAll()).ThrowsAsync(new Exception(error));

            var result = await inventoryController.GetAll();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(ControllerResponseStatus.Error));
            Assert.That(result.Data, Is.Empty);
            Assert.That(result.Error.Contains(error));
        }

        [Test]
        public async Task When_calling_get_all_inventory_and_nothing_is_returnedAsync()
        {
            mockInventoryRepository.Setup(x => x.GetAll()).ReturnsAsync(new List<Inventory>());

            var result = await inventoryController.GetAll();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(ControllerResponseStatus.Success));
            Assert.That(result.Data, Is.Empty);
            Assert.That(result.Error, Is.Empty);
        }

        [Test]
        public async Task When_calling_get_all_inventory_and_something_is_returnedAsync()
        {
            mockInventoryRepository.Setup(x => x.GetAll()).ReturnsAsync(new List<Inventory> { new() });

            var result = await inventoryController.GetAll();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(ControllerResponseStatus.Success));
            Assert.That(result.Data.Count, Is.GreaterThan(0));
            Assert.That(result.Error, Is.Empty);
        }

        [Test]
        public async Task When_calling_get_all_inventory_and_a_single_inventory_is_returnedAsync()
        {
            var expectedInventory = new Inventory("526485157884", "title", "description", 5, "image.url", ["first", "second"]);
            var expectedResponse = new InventoryControllerResponse(ControllerResponseStatus.Success, [expectedInventory]);
            mockInventoryRepository.Setup(x => x.GetAll()).ReturnsAsync([expectedInventory]);

            var result = await inventoryController.GetAll();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(expectedResponse.Status));
            Assert.That(result.Data.Count, Is.EqualTo(1));
            Assert.That(result.Data, Is.EquivalentTo(expectedResponse.Data));
            Assert.That(result.Error, Is.EqualTo(expectedResponse.Error));
        }

        [Test]
        public async Task When_calling_get_all_inventory_and_multiple_inventories_are_returnedAsync()
        {
            var expectedInventories = new List<Inventory>
            {
                new Inventory("526485157884", "title1", "description1", 5, "image.url/1", ["first", "second"]),
                new Inventory("846357158269", "title2", "description2", 2, "image.url/2", ["first", "second"])
            };
            var expectedResponse = new InventoryControllerResponse(ControllerResponseStatus.Success, expectedInventories);

            mockInventoryRepository.Setup(x => x.GetAll()).ReturnsAsync(expectedInventories);

            var result = await inventoryController.GetAll();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(expectedResponse.Status));
            Assert.That(result.Data.Count, Is.EqualTo(expectedInventories.Count));
            Assert.That(result.Data, Is.EquivalentTo(expectedResponse.Data));
            Assert.That(result.Error, Is.EqualTo(expectedResponse.Error));
        }

        [Test]
        public async Task When_calling_get_inventory_and_there_is_an_errorAsync()
        {
            var barcode = "526485157884";
            var error = "An error occurred.";
            mockInventoryRepository.Setup(x => x.Get(barcode)).ThrowsAsync(new Exception(error));

            var result = await inventoryController.Get(barcode);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(ControllerResponseStatus.Error));
            Assert.That(result.Data, Is.Empty);
            Assert.That(result.Error.Contains(error));
        }

        [Test]
        public async Task When_calling_get_inventory_and_nothing_is_returnedAsync()
        {
            var barcode = "526485157884";
            var expectedResponse = new InventoryControllerResponse(ControllerResponseStatus.NotFound, new List<Inventory>());
            mockInventoryRepository.Setup(x => x.Get(barcode)).ReturnsAsync((Inventory)null);

            var result = await inventoryController.Get(barcode);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(expectedResponse.Status));
            Assert.That(result.Data, Is.Empty);
            Assert.That(result.Error, Is.Empty);
        }

        [Test]
        public async Task When_calling_get_inventory_and_something_is_returnedAsync()
        {
            var expectedInventory = new Inventory("526485157884", "title", "description", 5, "image.url", ["first", "second"]);
            var expectedResponse = new InventoryControllerResponse(ControllerResponseStatus.Success, [expectedInventory]);
            mockInventoryRepository.Setup(x => x.Get(expectedInventory.Barcode)).ReturnsAsync(expectedInventory);

            var result = await inventoryController.Get(expectedInventory.Barcode);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(ControllerResponseStatus.Success));
            Assert.That(result.Data.Count, Is.EqualTo(1));
            Assert.That(result.Data, Is.EquivalentTo(expectedResponse.Data));
            Assert.That(result.Error, Is.EqualTo(expectedResponse.Error));
        }

        [Test]
        public async Task When_calling_add_inventory_and_there_is_an_errorAsync()
        {
            var inventory = new Inventory("526485157884", "title", "description", 5, "image.url", ["first", "second"]);
            var error = "An error occurred.";
            mockInventoryRepository.Setup(x => x.Insert(inventory)).ThrowsAsync(new Exception(error));

            var result = await inventoryController.Add(inventory);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(ControllerResponseStatus.Error));
            Assert.That(result.Data, Is.Empty);
            Assert.That(result.Error.Contains(error));
        }

        [Test]
        public async Task When_calling_add_inventory_successfullyAsync()
        {
            var inventory = new Inventory("526485157884", "title", "description", 5, "image.url", ["first", "second"]);
            mockInventoryRepository.Setup(x => x.Insert(inventory));
            mockInventoryRepository.Setup(x => x.Get(inventory.Barcode)).ReturnsAsync(inventory);

            var result = await inventoryController.Add(inventory);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(ControllerResponseStatus.Success));
            Assert.That(result.Data, Is.EqualTo(new List<Inventory>() { inventory }));
            Assert.That(result.Error, Is.Empty);
        }

        [TearDown]
        public void TearDown()
        {
            inventoryController.Dispose();
        }
    }
}