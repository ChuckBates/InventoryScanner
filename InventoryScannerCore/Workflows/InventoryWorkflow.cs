using InventoryScannerCore.Enums;
using InventoryScannerCore.Lookups;
using InventoryScannerCore.Models;
using InventoryScannerCore.Repositories;
using InventoryScannerCore.Workflows;

namespace InventoryScannerCore.UnitTests
{
    public class InventoryWorkflow
    {
        private IInventoryRepository inventoryRepository;
        private IBarcodeLookup barcodeLookup;
        private IImageLookup imageLookup;
        private IImageRepository imageRepository;

        public InventoryWorkflow(IInventoryRepository inventoryRepository, IBarcodeLookup barcodeLookup, IImageLookup imageLookup, IImageRepository imageRepository)
        {
            this.inventoryRepository = inventoryRepository;
            this.barcodeLookup = barcodeLookup;
            this.imageLookup = imageLookup;
            this.imageRepository = imageRepository;
        }

        public async Task<InventoryWorkflowResponse> Add(Inventory inventory)
        {
            var errorMessages = new List<string>();
            var barcode = await barcodeLookup.Get(inventory.Barcode);
            if (barcode == null)
            {
                errorMessages.Add("Error looking up barcode: Barcode not found.");
                return new InventoryWorkflowResponse(WorkflowResponseStatus.Error, null, errorMessages);
            }

            if (barcode.product.images.Length > 0)
            {
                var (imagePath, errors) = await SaveImage(barcode);
                if (errors.Count > 0)
                {
                    errorMessages.AddRange(errors);
                }
                inventory.ImagePath = imagePath;
            }
            else
            {
                errorMessages.Add("Error looking up barcode: Image not found.");
            }

            var updatedInventory = UpdateInventoryFromBarcode(inventory, barcode);
            var rowsAffected = await inventoryRepository.Insert(updatedInventory);
            if (rowsAffected == 0)
            {
                errorMessages.Add("Error looking up barcode: Failed to save inventory.");
                return new InventoryWorkflowResponse(WorkflowResponseStatus.Error, null, errorMessages);
            }

            return new InventoryWorkflowResponse(WorkflowResponseStatus.Success, updatedInventory, errorMessages);
        }

        private static Inventory UpdateInventoryFromBarcode(Inventory inventory, Barcode barcode) => new()
        {
            Barcode = inventory.Barcode,
            Quantity = inventory.Quantity,
            Title = barcode.product.title,
            Description = barcode.product.description,
            ImagePath = inventory.ImagePath,
            Categories = inventory.Categories
        };

        private async Task<(string imagePath, List<string> errors)> SaveImage(Barcode barcode)
        {
            var result = (imagePath: string.Empty, errors: new List<string>());

            var imageStream = await imageLookup.Get(barcode.product.images[0]);
            if (imageStream != null)
            {
                var imagePath = $"/Images/{barcode.product.title}-{barcode.product.barcode}.jpeg";
                var saveResult = await imageRepository.Insert(imageStream, imagePath);
                if (saveResult != "success")
                {
                    result.errors.Add("Error looking up barcode: Failed to save image.");
                }
                else
                {
                    result.imagePath = imagePath;
                }
            }
            else
            {
                result.errors.Add("Error looking up barcode: Image retrieval failed.");
            }

            return result;
        }
    }
}