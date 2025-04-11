using InventoryScanner.Core.Lookups;
using InventoryScanner.Core.Messages;
using InventoryScanner.Core.Models;
using InventoryScanner.Core.Repositories;
using System.Text.RegularExpressions;

namespace InventoryScanner.Core.Handlers
{
    public class FetchInventoryMetadataMessageHandler : IFetchInventoryMetadataMessageHandler
    {
        private readonly IBarcodeLookup barcodeLookup;
        private readonly IImageLookup imageLookup;
        private readonly IImageRepository imageRepository;
        private readonly IInventoryRepository inventoryRepository;
        private readonly ILogger<FetchInventoryMetadataMessageHandler> logger;

        public FetchInventoryMetadataMessageHandler(
            IBarcodeLookup barcodeLookup, 
            IImageLookup imageLookup, 
            IImageRepository imageRepository, 
            IInventoryRepository inventoryRepository, 
            ILogger<FetchInventoryMetadataMessageHandler> logger)
        {
            this.barcodeLookup = barcodeLookup;
            this.imageLookup = imageLookup;
            this.imageRepository = imageRepository;
            this.inventoryRepository = inventoryRepository;
            this.logger = logger;
        }

        public async Task Handle(FetchInventoryMetadataMessage message)
        {
            await FetchDetails(message);
        }

        private async Task FetchDetails(FetchInventoryMetadataMessage message)
        {
            var inventory = await inventoryRepository.Get(message.Barcode) ?? throw new Exception("Error handling metadata update message: Inventory not found.");
            var barcode = await barcodeLookup.Get(message.Barcode) ?? throw new Exception("Error handling metadata update message: Barcode not found.");

            if (barcode.product.images.Length > 0)
            {
                var (imagePath, errors) = await SaveImage(barcode);
                if (errors.Count > 0)
                {
                    var errorExceptions = new List<Exception>();
                    foreach (var error in errors)
                    {
                        errorExceptions.Add(new Exception(error));
                    }
                    logger.LogError("Error handling metadata update message: {Errors} ", string.Join(", ", errorExceptions));
                }
                inventory.ImagePath = imagePath;
            }

            var updatedInventory = UpdateInventoryFromBarcode(inventory, barcode);

            var rowsAffected = await inventoryRepository.Insert(updatedInventory);
            if (rowsAffected == 0)
            {
                throw new Exception("Error handling metadata update message: Failed to save inventory.");
            }
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
