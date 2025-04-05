using InventoryScanner.Core.Enums;
using InventoryScanner.Core.Lookups;
using InventoryScanner.Core.Models;
using InventoryScanner.Core.Repositories;
using InventoryScanner.Core.Workflows;
using System.Text.RegularExpressions;

namespace InventoryScanner.Core.UnitTests
{
    public class InventoryWorkflow : IInventoryWorkflow
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

        public async Task<InventoryWorkflowResponse> Get(string barcode)
        {
            var response = new InventoryWorkflowResponse(WorkflowResponseStatus.Success, [], []);
            try
            {
                var inventory = await inventoryRepository.Get(barcode);
                if (inventory == null) {
                    response.Status = WorkflowResponseStatus.Error;
                    response.Errors.Add("Error looking up barcode: Inventory not found.");
                }
                else
                {
                    response.Data.Add(inventory);
                }
            }
            catch (Exception)
            {
                response.Status = WorkflowResponseStatus.Error;
                response.Errors.Add("Error looking up barcode: Failed to retrieve inventory.");
            }

            return response;
        }

        public async Task<InventoryWorkflowResponse> GetAll()
        {
            var response = new InventoryWorkflowResponse(WorkflowResponseStatus.Success, [], []);
            try
            {
                var inventories = await inventoryRepository.GetAll();
                if (inventories == null || inventories.ToList().Count == 0)
                {
                    response.Status = WorkflowResponseStatus.Error;
                    response.Errors.Add("Error looking up all inventory: Inventory not found.");
                }
                else
                {
                    response.Data.AddRange(inventories);
                }
            }
            catch (Exception)
            {
                response.Status = WorkflowResponseStatus.Error;
                response.Errors.Add("Error looking up all inventory: Failed to retrieve inventory.");
            }

            return response;
        }

        public async Task<InventoryWorkflowResponse> Add(Inventory inventory)
        {
            var fetchResponse = await FetchDetails(inventory);
            if (fetchResponse.Status == WorkflowResponseStatus.Error)
            {
                return fetchResponse;
            }

            var rowsAffected = await inventoryRepository.Insert(fetchResponse.Data.First());
            if (rowsAffected == 0)
            {
                fetchResponse.Status = WorkflowResponseStatus.Error;
                fetchResponse.Data.Clear();
                fetchResponse.Errors.Add("Error looking up barcode: Failed to save inventory.");
                return fetchResponse;
            }

            return fetchResponse;
        }

        public async Task<InventoryWorkflowResponse> Update(Inventory inventory, bool refetch = false)
        {
            var response = new InventoryWorkflowResponse(WorkflowResponseStatus.Success, [inventory], []);
            if (refetch)
            {
                response = await FetchDetails(inventory);
                if (response.Status == WorkflowResponseStatus.Error)
                {
                    return response;
                }
            }

            var rowsAffected = await inventoryRepository.Insert(response.Data.First());
            if (rowsAffected == 0)
            {
                response.Status = WorkflowResponseStatus.Error;
                response.Errors.Add("Error looking up barcode: Failed to update inventory.");
            }

            response.Data.Clear();
            var updatedInventoy = await inventoryRepository.Get(inventory.Barcode);
            if (updatedInventoy == null)
            {
                response.Status = WorkflowResponseStatus.Error;
                response.Errors.Add("Error looking up barcode: Failed to retrieve updated inventory.");
            }
            else
            {
                response.Data.Add(updatedInventoy);
            }

            return response;
        }

        private async Task<InventoryWorkflowResponse> FetchDetails(Inventory inventory)
        {
            var errorMessages = new List<string>();
            var barcode = await barcodeLookup.Get(inventory.Barcode);
            if (barcode == null)
            {
                errorMessages.Add("Error looking up barcode: Barcode not found.");
                return new InventoryWorkflowResponse(WorkflowResponseStatus.Error, [], errorMessages);
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

            return new InventoryWorkflowResponse(WorkflowResponseStatus.Success, [updatedInventory], errorMessages);
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
            var imageUrl = barcode.product.images[0];
            var extension = Regex.Match(imageUrl, "[^.]+$");

            var imageStream = await imageLookup.Get(barcode.product.images[0]);
            if (imageStream != null)
            {
                var imagePath = Directory.GetCurrentDirectory() + $"/Images/{barcode.product.title}-{barcode.product.barcode}.{extension.Value ?? "jpg"}"; 
                var despacedImagePath = imagePath.Replace(" ", "");
                var saveResult = await imageRepository.Insert(imageStream, despacedImagePath);
                if (saveResult != "success")
                {
                    result.errors.Add("Error looking up barcode: Failed to save image.");
                }
                else
                {
                    result.imagePath = despacedImagePath;
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