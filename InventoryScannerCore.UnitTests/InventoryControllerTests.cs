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
        public void When_calling_get_all_inventory_and_there_is_an_error()
        {
            var error = "An error occurred.";
            mockInventoryRepository.Setup(x => x.GetAll()).Throws(new Exception(error));

            var result = inventoryController.GetAll();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(ControllerResponseStatus.Error));
            Assert.That(result.Data, Is.Empty);
            Assert.That(result.Error.Contains(error));
        }

        [Test]
        public void When_calling_get_all_inventory_and_nothing_is_returned()
        {
            mockInventoryRepository.Setup(x => x.GetAll()).Returns(new List<Inventory>());

            var result = inventoryController.GetAll();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(ControllerResponseStatus.Success));
            Assert.That(result.Data, Is.Empty);
            Assert.That(result.Error, Is.Empty);
        }

        [Test]
        public void When_calling_get_all_inventory_and_something_is_returned()
        {
            mockInventoryRepository.Setup(x => x.GetAll()).Returns(new List<Inventory> { new() });

            var result = inventoryController.GetAll();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(ControllerResponseStatus.Success));
            Assert.That(result.Data.Count, Is.GreaterThan(0));
            Assert.That(result.Error, Is.Empty);
        }

        [Test]
        public void When_calling_get_all_inventory_and_a_single_inventory_is_returned()
        {
            var expectedInventory = new Inventory("526485157884", "title", "description", 5, "image.url", ["first", "second"]);
            var expectedResponse = new InventoryControllerResponse(ControllerResponseStatus.Success, [expectedInventory]);
            mockInventoryRepository.Setup(x => x.GetAll()).Returns([expectedInventory]);

            var result = inventoryController.GetAll();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(expectedResponse.Status));
            Assert.That(result.Data.Count, Is.EqualTo(1));
            Assert.That(result.Data, Is.EquivalentTo(expectedResponse.Data));
            Assert.That(result.Error, Is.EqualTo(expectedResponse.Error));
        }

        [Test]
        public void When_calling_get_all_inventory_and_multiple_inventories_are_returned()
        {
            var expectedInventories = new List<Inventory>
            {
                new Inventory("526485157884", "title1", "description1", 5, "image.url/1", ["first", "second"]),
                new Inventory("846357158269", "title2", "description2", 2, "image.url/2", ["first", "second"])
            };
            var expectedResponse = new InventoryControllerResponse(ControllerResponseStatus.Success, expectedInventories);

            mockInventoryRepository.Setup(x => x.GetAll()).Returns(expectedInventories);

            var result = inventoryController.GetAll();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(expectedResponse.Status));
            Assert.That(result.Data.Count, Is.EqualTo(expectedInventories.Count));
            Assert.That(result.Data, Is.EquivalentTo(expectedResponse.Data));
            Assert.That(result.Error, Is.EqualTo(expectedResponse.Error));
        }

        [Test]
        public void When_calling_get_inventory_and_there_is_an_error()
        {
            var barcode = "526485157884";
            var error = "An error occurred.";
            mockInventoryRepository.Setup(x => x.Get(barcode)).Throws(new Exception(error));

            var result = inventoryController.Get(barcode);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(ControllerResponseStatus.Error));
            Assert.That(result.Data, Is.Empty);
            Assert.That(result.Error.Contains(error));
        }

        [Test]
        public void When_calling_get_inventory_and_nothing_is_returned()
        {
            var barcode = "526485157884";
            var expectedResponse = new InventoryControllerResponse(ControllerResponseStatus.NotFound, new List<Inventory>());
            mockInventoryRepository.Setup(x => x.Get(barcode)).Returns((Inventory)null);

            var result = inventoryController.Get(barcode);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(expectedResponse.Status));
            Assert.That(result.Data, Is.Empty);
            Assert.That(result.Error, Is.Empty);
        }

        [Test]
        public void When_calling_get_inventory_and_something_is_returned()
        {
            var expectedInventory = new Inventory("526485157884", "title", "description", 5, "image.url", ["first", "second"]);
            var expectedResponse = new InventoryControllerResponse(ControllerResponseStatus.Success, [expectedInventory]);
            mockInventoryRepository.Setup(x => x.Get(expectedInventory.Barcode)).Returns(expectedInventory);

            var result = inventoryController.Get(expectedInventory.Barcode);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(ControllerResponseStatus.Success));
            Assert.That(result.Data.Count, Is.EqualTo(1));
            Assert.That(result.Data, Is.EquivalentTo(expectedResponse.Data));
            Assert.That(result.Error, Is.EqualTo(expectedResponse.Error));
        }

        [Test]
        public void When_calling_add_inventory_and_there_is_an_error()
        {
            var inventory = new Inventory("526485157884", "title", "description", 5, "image.url", ["first", "second"]);
            var error = "An error occurred.";
            mockInventoryRepository.Setup(x => x.Insert(inventory)).Throws(new Exception(error));

            var result = inventoryController.Add(inventory);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(ControllerResponseStatus.Error));
            Assert.That(result.Data, Is.Empty);
            Assert.That(result.Error.Contains(error));
        }

        [Test]
        public void When_calling_add_inventory_successfully()
        {
            var inventory = new Inventory("526485157884", "title", "description", 5, "image.url", ["first", "second"]);
            mockInventoryRepository.Setup(x => x.Insert(inventory));

            var result = inventoryController.Add(inventory);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(ControllerResponseStatus.Success));
            Assert.That(result.Data, Is.Empty);
            Assert.That(result.Error, Is.Empty);
        }

        [TearDown]
        public void TearDown()
        {
            inventoryController.Dispose();
        }
    }
}