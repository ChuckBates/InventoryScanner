using InventoryScanner.Core.Messages;
using InventoryScanner.Core.Models;
using InventoryScanner.Core.Repositories;
using InventoryScanner.Core.Publishers.Interfaces;
using System.Text.RegularExpressions;
using InventoryScanner.Messaging.Enums;
using InventoryScanner.Logging;
using InventoryScanner.Core.Wrappers;

namespace InventoryScanner.Core.Handlers
{
    public class FetchInventoryMetadataMessageHandler : IFetchInventoryMetadataMessageHandler
    {
        private readonly IBarcodeWrapper barcodeWrapper;
        private readonly IImageWrapper imageWrapper;
        private readonly IImageRepository imageRepository;
        private readonly IInventoryRepository inventoryRepository;
        private readonly IInventoryUpdatedPublisher inventoryUpdatedPublisher;
        private readonly IAppLogger<FetchInventoryMetadataMessageHandler> logger;

        public FetchInventoryMetadataMessageHandler(
            IBarcodeWrapper barcodeWrapper,
            IImageWrapper imageWrapper,
            IImageRepository imageRepository,
            IInventoryRepository inventoryRepository,
            IInventoryUpdatedPublisher inventoryUpdatedPublisher,
            IAppLogger<FetchInventoryMetadataMessageHandler> logger)
        {
            this.barcodeWrapper = barcodeWrapper;
            this.imageWrapper = imageWrapper;
            this.imageRepository = imageRepository;
            this.inventoryRepository = inventoryRepository;
            this.inventoryUpdatedPublisher = inventoryUpdatedPublisher;
            this.logger = logger;
        }

        public async Task Handle(FetchInventoryMetadataMessage message)
        {
            await FetchDetails(message);
        }

        private async Task FetchDetails(FetchInventoryMetadataMessage message)
        {
            var inventory = await inventoryRepository.Get(message.Barcode) ?? throw new Exception("Error handling metadata update message: Inventory not found.");
            var barcode = await barcodeWrapper.Get(message.Barcode) ?? throw new Exception("Error handling metadata update message: Barcode not found.");

            if (barcode.product.images.Length > 0)
            {                
                inventory.ImagePath = await SaveImage(barcode);
            }

            var updatedInventory = UpdateInventoryFromBarcode(inventory, barcode);

            var rowsAffected = await inventoryRepository.Insert(updatedInventory);
            if (rowsAffected == 0)
            {
                throw new Exception("Error handling metadata update message: Failed to save inventory.");
            }

            var publishResponse = await inventoryUpdatedPublisher.Publish(updatedInventory);
            if (publishResponse.Status != PublisherResponseStatus.Success)
            {
                logger.Warning(new LogContext
                {
                    Barcode = barcode.product.barcode,
                    Component = typeof(FetchInventoryMetadataMessageHandler).Name,
                    Message = "Error handling metadata update message: Failed to publish inventory update.",
                    Operation = "Fetch Details"
                });
            }
        }

        private static Inventory UpdateInventoryFromBarcode(Inventory inventory, Barcode barcode) => new()
        {
            Barcode = inventory.Barcode,
            Quantity = inventory.Quantity,
            Title = barcode.product.title,
            Description = barcode.product.description,
            ImagePath = inventory.ImagePath,
            Categories = inventory.Categories,
            UpdatedAt = DateTime.UtcNow
        };

        private async Task<string> SaveImage(Barcode barcode)
        {
            var result = string.Empty;
            var imageUrl = barcode.product.images[0];
            var extension = Regex.Match(imageUrl, "[^.]+$");

            var imageStream = await imageWrapper.Get(barcode.product.images[0]);
            if (imageStream != null)
            {
                var imagePath = Directory.GetCurrentDirectory() + $"/Images/{barcode.product.title}-{barcode.product.barcode}.{extension.Value ?? "jpg"}";
                var despacedImagePath = imagePath.Replace(" ", "");
                var saveResult = await imageRepository.Insert(imageStream, despacedImagePath);
                if (saveResult != "success")
                {
                    logger.Warning(new LogContext
                    {
                        Barcode = barcode.product.barcode,
                        Component = typeof(FetchInventoryMetadataMessageHandler).Name,
                        Message = "Error looking up barcode: Failed to save image.",
                        Operation = "Save Image"
                    });
                }
                else
                {
                    result = despacedImagePath;
                }
            }
            else
            {
                logger.Warning(new LogContext
                {
                    Barcode = barcode.product.barcode,
                    Component = typeof(FetchInventoryMetadataMessageHandler).Name,
                    Message = "Error looking up barcode: Image retrieval failed.",
                    Operation = "Save Image"
                });
            }

            return result;
        }
    }
}
