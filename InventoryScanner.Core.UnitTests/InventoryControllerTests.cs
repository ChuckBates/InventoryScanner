using InventoryScanner.Core.Controllers;
using InventoryScanner.Core.Enums;
using InventoryScanner.Core.Models;
using InventoryScanner.Core.Repositories;
using InventoryScanner.Core.Workflows;
using InventoryScanner.TestUtilities;
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
            mockInventoryWorkflow.Setup(x => x.GetAll(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>())).ThrowsAsync(new Exception(error));

            var result = await inventoryController.GetAll();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(ControllerResponseStatus.Error));
            Assert.That(result.Data, Is.Empty);
            Assert.That(result.Error.Contains(error));
        }

        [Test]
        public async Task When_calling_get_all_inventory_and_nothing_is_returned()
        {
            mockInventoryWorkflow.Setup(x => x.GetAll(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(new InventoryWorkflowResponse(WorkflowResponseStatus.Success, [], []));

            var result = await inventoryController.GetAll();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(ControllerResponseStatus.Success));
            Assert.That(result.Data, Is.Empty);
            Assert.That(result.Error, Is.Empty);
        }

        [Test]
        public async Task When_calling_get_all_inventory_and_something_is_returned()
        {
            var expectedWorkflowResponse = new InventoryWorkflowResponse(WorkflowResponseStatus.Success, [new Inventory("526485157884", "title", "description", 5, "image.url", ["first", "second"], DateTime.UtcNow)], []);
            mockInventoryWorkflow.Setup(x => x.GetAll(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(expectedWorkflowResponse);

            var result = await inventoryController.GetAll();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(ControllerResponseStatus.Success));
            Assert.That(result.Data.Count, Is.GreaterThan(0));
            Assert.That(result.Error, Is.Empty);
        }

        [Test]
        public async Task When_calling_get_all_inventory_and_a_single_inventory_is_returned()
        {
            var expectedWorkflowResponse = new InventoryWorkflowResponse(WorkflowResponseStatus.Success, [new Inventory("526485157884", "title", "description", 5, "image.url", ["first", "second"], DateTime.UtcNow)], []);
            var expectedResponse = new InventoryControllerResponse(ControllerResponseStatus.Success, expectedWorkflowResponse.Data);
            mockInventoryWorkflow.Setup(x => x.GetAll(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(expectedWorkflowResponse);

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
                new Inventory(Barcodes.Generate(), "title1", "description1", 5, "image.url/1", ["first", "second"], DateTime.UtcNow),
                new Inventory(Barcodes.Generate(), "title2", "description2", 2, "image.url/2", ["first", "second"], DateTime.UtcNow)
            };
            var expectedWorkflowResponse = new InventoryWorkflowResponse(WorkflowResponseStatus.Success, expectedInventories, []);
            var expectedResponse = new InventoryControllerResponse(ControllerResponseStatus.Success, expectedInventories);

            mockInventoryWorkflow.Setup(x => x.GetAll(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(expectedWorkflowResponse);

            var result = await inventoryController.GetAll();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(expectedResponse.Status));
            Assert.That(result.Data.Count, Is.EqualTo(expectedInventories.Count));
            Assert.That(result.Data, Is.EquivalentTo(expectedResponse.Data));
            Assert.That(result.Error, Is.EqualTo(expectedResponse.Error));
        }

        [Test]
        public async Task When_calling_get_all_inventory_and_multiple_inventories_are_filtered()
        {
            var since = DateTime.UtcNow.AddHours(-1);
            var page = 1;
            var pageSize = 50;
            var expectedInventories = new List<Inventory>
            {
                new Inventory(Barcodes.Generate(), "title1", "description1", 5, "image.url/1", ["first", "second"], DateTime.UtcNow.AddDays(-5)),
                new Inventory(Barcodes.Generate(), "title2", "description2", 2, "image.url/2", ["first", "second"], DateTime.UtcNow)
            };
            var expectedWorkflowResponse = new InventoryWorkflowResponse(WorkflowResponseStatus.Success, [expectedInventories[1]], []);
            var expectedResponse = new InventoryControllerResponse(ControllerResponseStatus.Success, [expectedInventories[1]]);

            mockInventoryWorkflow.Setup(x => x.GetAll(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(expectedWorkflowResponse);

            var result = await inventoryController.GetAll(since, page, pageSize);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(expectedResponse.Status));
            Assert.That(result.Data.Count, Is.EqualTo(1));
            Assert.That(result.Data, Is.EquivalentTo(expectedResponse.Data));
            Assert.That(result.Error, Is.EqualTo(expectedResponse.Error));
        }

        [Test]
        public async Task When_calling_get_all_inventory_and_more_are_available_after_first_page()
        {
            var since = DateTime.MinValue;
            var page = 1;
            var pageSize = 5;
            var hasMore = true;
            var expectedInventories = new List<Inventory>
            {
                new Inventory(Barcodes.Generate(), "title1", "description1", 5, "image.url/1", ["first", "second"], DateTime.UtcNow),
                new Inventory(Barcodes.Generate(), "title2", "description2", 2, "image.url/2", ["first", "second"], DateTime.UtcNow),
                new Inventory(Barcodes.Generate(), "title3", "description3", 4, "image.url/3", ["first", "second"], DateTime.UtcNow),
                new Inventory(Barcodes.Generate(), "title4", "description4", 7, "image.url/4", ["first", "second"], DateTime.UtcNow),
                new Inventory(Barcodes.Generate(), "title5", "description5", 2, "image.url/5", ["first", "second"], DateTime.UtcNow),
                new Inventory(Barcodes.Generate(), "title6", "description6", 9, "image.url/6", ["first", "second"], DateTime.UtcNow)
            };
            var expectedWorkflowResponse = new InventoryWorkflowResponse(WorkflowResponseStatus.Success, expectedInventories, []);
            var expectedResponse = new InventoryControllerPaginatedResponse(ControllerResponseStatus.Success, expectedInventories.Take(pageSize).ToList(), string.Empty, page, pageSize, hasMore);

            mockInventoryWorkflow.Setup(x => x.GetAll(since, page, pageSize)).ReturnsAsync(expectedWorkflowResponse);

            var result = await inventoryController.GetAll(since, page, pageSize);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(expectedResponse.Status));
            Assert.That(result.Data.Count, Is.EqualTo(pageSize));
            Assert.That(result.Data, Is.EquivalentTo(expectedResponse.Data));
            Assert.That(result.Error, Is.EqualTo(expectedResponse.Error));
            Assert.That(result.Page, Is.EqualTo(expectedResponse.Page));
            Assert.That(result.PageSize, Is.EqualTo(expectedResponse.PageSize));
            Assert.That(result.HasMore, Is.EqualTo(expectedResponse.HasMore));
        }

        [Test]
        public async Task When_calling_get_all_inventory_and_no_more_are_available()
        {
            var since = DateTime.MinValue;
            var page = 2;
            var pageSize = 3;
            var hasMore = false;
            var expectedCount = 2;
            var expectedInventories = new List<Inventory>
            {
                new Inventory(Barcodes.Generate(), "title1", "description1", 5, "image.url/1", ["first", "second"], DateTime.UtcNow),
                new Inventory(Barcodes.Generate(), "title2", "description2", 2, "image.url/2", ["first", "second"], DateTime.UtcNow)
            };
            var expectedWorkflowResponse = new InventoryWorkflowResponse(WorkflowResponseStatus.Success, expectedInventories, []);
            var expectedResponse = new InventoryControllerPaginatedResponse(ControllerResponseStatus.Success, expectedInventories.Take(pageSize).ToList(), string.Empty, page, pageSize, hasMore);

            mockInventoryWorkflow.Setup(x => x.GetAll(since, page, pageSize)).ReturnsAsync(expectedWorkflowResponse);

            var result = await inventoryController.GetAll(since, page, pageSize);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(expectedResponse.Status));
            Assert.That(result.Data.Count, Is.EqualTo(expectedCount));
            Assert.That(result.Data, Is.EquivalentTo(expectedResponse.Data));
            Assert.That(result.Error, Is.EqualTo(expectedResponse.Error));
            Assert.That(result.Page, Is.EqualTo(expectedResponse.Page));
            Assert.That(result.PageSize, Is.EqualTo(expectedResponse.PageSize));
            Assert.That(result.HasMore, Is.EqualTo(expectedResponse.HasMore));
        }

        [Test]
        public async Task When_calling_get_inventory_and_there_is_an_error()
        {
            var barcode = Barcodes.Generate();
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
            var barcode = Barcodes.Generate();
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
            var expectedInventory = new Inventory(Barcodes.Generate(), "title", "description", 5, "image.url", ["first", "second"], DateTime.UtcNow);
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
            var barcode = Barcodes.Generate();
            var inventory = new Inventory(barcode, "", "", 5, "", [], DateTime.UtcNow);
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
            var barcode = Barcodes.Generate();
            var inventory = new Inventory(barcode, "", "", 5, "", [], DateTime.UtcNow);
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
            var barcode = Barcodes.Generate();
            var inventory = new Inventory(barcode, "", "", 5, "", [], DateTime.UtcNow);
            var expectedInventory = new Inventory(barcode, "title", "description", 5, "image.url", ["first", "second"], DateTime.UtcNow);
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
            var inventory = new Inventory(Barcodes.Generate(), "", "", 5, "", [], DateTime.UtcNow);
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
            var inventory = new Inventory(Barcodes.Generate(), "", "", 5, "", [], DateTime.UtcNow);
            var updatedInventory = new Inventory(Barcodes.Generate(), "title", "description", 5, "image.url", ["first", "second"], DateTime.UtcNow);
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