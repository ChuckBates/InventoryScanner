using InventoryScannerCore.Controllers;
using InventoryScannerCore.Models;
using InventoryScannerCore.Repositories;
using Moq;

namespace InventoryScannerCore.ApiTests
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
        public void When_calling_get_all_inventory_and_something_is_returned()
        {
            mockInventoryRepository.Setup(x => x.GetAll()).Returns(new List<Inventory> { new Inventory() });

            var result = inventoryController.GetAll();

            Assert.IsNotEmpty(result);
        }

        [Test]
        public void When_calling_get_all_inventory_and_a_single_inventory_is_returned_with_no_quantity()
        {
            mockInventoryRepository.Setup(x => x.GetAll()).Returns(new List<Inventory> { new Inventory() { Quantity = 0 } });

            var result = inventoryController.GetAll();

            Assert.That(result.First().Quantity, Is.EqualTo(0));
        }

        [Test]
        public void When_calling_get_all_inventory_and_a_single_inventory_is_returned_with_non_zero_quantity()
        {
            mockInventoryRepository.Setup(x => x.GetAll()).Returns(new List<Inventory> { new Inventory() { Quantity = 1 } });

            var result = inventoryController.GetAll();

            Assert.That(result.First().Quantity, Is.GreaterThan(0));
        }

        [Test]
        public void When_calling_get_all_inventory_and_multiple_inventories_are_returned_with_non_zero_quantities()
        {
            var inventories = new List<Inventory>
            {
                new() { Quantity = 1 },
                new() { Quantity = 2 },
                new() { Quantity = 3 }
            };

            mockInventoryRepository.Setup(x => x.GetAll()).Returns(inventories);

            var result = inventoryController.GetAll();

            Assert.That(result.Count(), Is.EqualTo(3));
            Assert.That(result.First().Quantity, Is.EqualTo(1));
            Assert.That(result.Skip(1).First().Quantity, Is.EqualTo(2));
        }

        [TearDown]
        public void TearDown()
        {
            inventoryController.Dispose();
        }
    }
}