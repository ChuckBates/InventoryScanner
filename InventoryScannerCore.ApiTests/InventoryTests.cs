using InventoryScannerCore.Controllers;

namespace InventoryScannerCore.ApiTests
{
    [TestFixture]
    public class InventoryTests
    {
        InventoryController inventoryController;

        [SetUp]
        public void Setup()
        {
            inventoryController = new InventoryController();
        }

        [Test]
        public void When_calling_get_all_inventory_and_something_is_returned()
        {
            var result = inventoryController.GetAll();

            Assert.IsNotEmpty(result);
        }

        [TearDown]
        public void TearDown()
        {
            inventoryController.Dispose();
        }
    }
}