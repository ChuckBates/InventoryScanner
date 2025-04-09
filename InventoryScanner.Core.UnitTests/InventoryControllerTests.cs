using InventoryScanner.Core.Controllers;
using InventoryScanner.Core.Enums;
using InventoryScanner.Core.Models;
using InventoryScanner.Core.Repositories;
using InventoryScanner.Core.Workflows;
using Moq;

namespace InventoryScanner.Core.UnitTests
{
    [TestFixture]
    public class InventoryControllerTests
    {
        InventoryController inventoryController;
        Mock<IInventoryWorkflow> mockInventoryWorkflow;

        [SetUp]
        public void Setup()
        {
            mockInventoryWorkflow = new Mock<IInventoryWorkflow>();
            inventoryController = new InventoryController(mockInventoryWorkflow.Object);
        }

        [Test]
        public async Task When_calling_get_all_inventory_and_there_is_an_error()
        {
            var error = "An error occurred.";
            mockInventoryWorkflow.Setup(x => x.GetAll()).ThrowsAsync(new Exception(error));

            var result = await inventoryController.GetAll();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(ControllerResponseStatus.Error));
            Assert.That(result.Data, Is.Empty);
            Assert.That(result.Error.Contains(error));
        }

        [Test]
        public async Task When_calling_get_all_inventory_and_nothing_is_returned()
        {
            mockInventoryWorkflow.Setup(x => x.GetAll()).ReturnsAsync(new InventoryWorkflowResponse(WorkflowResponseStatus.Success, [], []));

            var result = await inventoryController.GetAll();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(ControllerResponseStatus.Success));
            Assert.That(result.Data, Is.Empty);
            Assert.That(result.Error, Is.Empty);
        }

        [Test]
        public async Task When_calling_get_all_inventory_and_something_is_returned()
        {
            var expectedWorkflowResponse = new InventoryWorkflowResponse(WorkflowResponseStatus.Success, [new Inventory("526485157884", "title", "description", 5, "image.url", ["first", "second"])], []);
            mockInventoryWorkflow.Setup(x => x.GetAll()).ReturnsAsync(expectedWorkflowResponse);

            var result = await inventoryController.GetAll();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(ControllerResponseStatus.Success));
            Assert.That(result.Data.Count, Is.GreaterThan(0));
            Assert.That(result.Error, Is.Empty);
        }

        [Test]
        public async Task When_calling_get_all_inventory_and_a_single_inventory_is_returned()
        {
            var expectedWorkflowResponse = new InventoryWorkflowResponse(WorkflowResponseStatus.Success, [new Inventory("526485157884", "title", "description", 5, "image.url", ["first", "second"])], []);
            var expectedResponse = new InventoryControllerResponse(ControllerResponseStatus.Success, expectedWorkflowResponse.Data);
            mockInventoryWorkflow.Setup(x => x.GetAll()).ReturnsAsync(expectedWorkflowResponse);

            var result = await inventoryController.GetAll();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(expectedResponse.Status));
            Assert.That(result.Data.Count, Is.EqualTo(1));
            Assert.That(result.Data, Is.EquivalentTo(expectedResponse.Data));
            Assert.That(result.Error, Is.EqualTo(expectedResponse.Error));
        }

        [Test]
        public async Task When_calling_get_all_inventory_and_multiple_inventories_are_returned()
        {
            var expectedInventories = new List<Inventory>
            {
                new Inventory("526485157884", "title1", "description1", 5, "image.url/1", ["first", "second"]),
                new Inventory("846357158269", "title2", "description2", 2, "image.url/2", ["first", "second"])
            };
            var expectedWorkflowResponse = new InventoryWorkflowResponse(WorkflowResponseStatus.Success, expectedInventories, []);
            var expectedResponse = new InventoryControllerResponse(ControllerResponseStatus.Success, expectedInventories);

            mockInventoryWorkflow.Setup(x => x.GetAll()).ReturnsAsync(expectedWorkflowResponse);

            var result = await inventoryController.GetAll();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(expectedResponse.Status));
            Assert.That(result.Data.Count, Is.EqualTo(expectedInventories.Count));
            Assert.That(result.Data, Is.EquivalentTo(expectedResponse.Data));
            Assert.That(result.Error, Is.EqualTo(expectedResponse.Error));
        }

        [Test]
        public async Task When_calling_get_inventory_and_there_is_an_error()
        {
            var barcode = "526485157884";
            var error = "An error occurred.";
            mockInventoryWorkflow.Setup(x => x.Get(barcode)).ThrowsAsync(new Exception(error));

            var result = await inventoryController.Get(barcode);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(ControllerResponseStatus.Error));
            Assert.That(result.Data, Is.Empty);
            Assert.That(result.Error.Contains(error));
        }

        [Test]
        public async Task When_calling_get_inventory_and_nothing_is_returned()
        {
            var barcode = "526485157884";
            var expectedResponse = new InventoryControllerResponse(ControllerResponseStatus.NotFound, new List<Inventory>());
            var expectedWorkflowResponse = new InventoryWorkflowResponse(WorkflowResponseStatus.Success, [], []);
            mockInventoryWorkflow.Setup(x => x.Get(barcode)).ReturnsAsync(expectedWorkflowResponse);

            var result = await inventoryController.Get(barcode);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(expectedResponse.Status));
            Assert.That(result.Data, Is.Empty);
            Assert.That(result.Error, Is.Empty);
        }

        [Test]
        public async Task When_calling_get_inventory_and_something_is_returned()
        {
            var expectedInventory = new Inventory("526485157884", "title", "description", 5, "image.url", ["first", "second"]);
            var expectedWorkflowResponse = new InventoryWorkflowResponse(WorkflowResponseStatus.Success, [expectedInventory], []);
            var expectedResponse = new InventoryControllerResponse(ControllerResponseStatus.Success, [expectedInventory]);
            mockInventoryWorkflow.Setup(x => x.Get(expectedInventory.Barcode)).ReturnsAsync(expectedWorkflowResponse);

            var result = await inventoryController.Get(expectedInventory.Barcode);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(ControllerResponseStatus.Success));
            Assert.That(result.Data.Count, Is.EqualTo(1));
            Assert.That(result.Data, Is.EquivalentTo(expectedResponse.Data));
            Assert.That(result.Error, Is.EqualTo(expectedResponse.Error));
        }

        [Test]
        public async Task When_calling_update_inventory_and_there_is_an_error()
        {
            var barcode = "526485157884";
            var inventory = new Inventory(barcode, "", "", 5, "", []);
            var error = "An error occurred.";
            mockInventoryWorkflow.Setup(x => x.Update(It.IsAny<Inventory>(), It.IsAny<bool>())).ThrowsAsync(new Exception(error));

            var result = await inventoryController.Update(inventory, false);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(ControllerResponseStatus.Error));
            Assert.That(result.Data, Is.Empty);
            Assert.That(result.Error.Contains(error));
        }

        [Test]
        public async Task When_calling_update_inventory_and_nothing_is_returned()
        {
            var barcode = "526485157884";
            var inventory = new Inventory(barcode, "", "", 5, "", []);
            var expectedResponse = new InventoryControllerResponse(ControllerResponseStatus.NotFound, new List<Inventory>());
            var expectedWorkflowResponse = new InventoryWorkflowResponse(WorkflowResponseStatus.Success, [], []);
            mockInventoryWorkflow.Setup(x => x.Update(inventory, false)).ReturnsAsync(expectedWorkflowResponse);

            var result = await inventoryController.Update(inventory, false);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(expectedResponse.Status));
            Assert.That(result.Data, Is.Empty);
            Assert.That(result.Error, Is.Empty);
        }

        [Test]
        public async Task When_calling_update_inventory_and_something_is_returned()
        {
            var barcode = "526485157884";
            var inventory = new Inventory(barcode, "", "", 5, "", []);
            var expectedInventory = new Inventory(barcode, "title", "description", 5, "image.url", ["first", "second"]);
            var expectedWorkflowResponse = new InventoryWorkflowResponse(WorkflowResponseStatus.Success, [expectedInventory], []);
            var expectedResponse = new InventoryControllerResponse(ControllerResponseStatus.Success, [expectedInventory]);
            mockInventoryWorkflow.Setup(x => x.Update(inventory, false)).ReturnsAsync(expectedWorkflowResponse);

            var result = await inventoryController.Update(inventory, false);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(ControllerResponseStatus.Success));
            Assert.That(result.Data.Count, Is.EqualTo(1));
            Assert.That(result.Data, Is.EquivalentTo(expectedResponse.Data));
            Assert.That(result.Error, Is.EqualTo(expectedResponse.Error));
        }

        [Test]
        public async Task When_calling_add_inventory_and_there_is_an_error()
        {
            var inventory = new Inventory("526485157884", "", "", 5, "", []);
            var error = "An error occurred.";
            mockInventoryWorkflow.Setup(x => x.Add(inventory)).ReturnsAsync(new InventoryWorkflowResponse(WorkflowResponseStatus.Failure, null, new List<string> { error }));

            var result = await inventoryController.Add(inventory);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(ControllerResponseStatus.Error));
            Assert.That(result.Data, Is.Empty);
            Assert.That(result.Error.Contains(error));
        }

        [Test]
        public async Task When_calling_add_inventory_successfully()
        {
            var inventory = new Inventory("526485157884", "", "", 5, "", []);
            var updatedInventory = new Inventory("526485157884", "title", "description", 5, "image.url", ["first", "second"]);
            var expectedWorkflowResponse = new InventoryWorkflowResponse(WorkflowResponseStatus.Success, new List<Inventory> { updatedInventory }, new List<string>());
            mockInventoryWorkflow.Setup(x => x.Add(inventory)).ReturnsAsync(expectedWorkflowResponse);

            var result = await inventoryController.Add(inventory);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(ControllerResponseStatus.Success));
            Assert.That(result.Data, Is.EqualTo(new List<Inventory>() { updatedInventory }));
            Assert.That(result.Error, Is.Empty);
        }

        [TearDown]
        public void TearDown()
        {
            inventoryController.Dispose();
        }
    }
}