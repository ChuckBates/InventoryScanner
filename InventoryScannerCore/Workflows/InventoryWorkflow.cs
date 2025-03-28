using InventoryScannerCore.Lookups;
using InventoryScannerCore.Models;
using InventoryScannerCore.Repositories;

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

        public async Task<string> Add(Inventory inventory)
        {
            var barcode = await barcodeLookup.Get(inventory.Barcode);
            if (barcode == null)
            {
                return "Barcode not found";
            }

            var imagePath = await SaveImageAsync(barcode);
            inventory.ImagePath = imagePath;

            var updatedInventory = UpdateInventory(inventory, barcode);
            inventoryRepository.Insert(updatedInventory);

            return "success";
        }

        private Inventory UpdateInventory(Inventory inventory, Barcode barcode)
        {
            return new Inventory
            {
                Barcode = inventory.Barcode,
                Quantity = inventory.Quantity,
                Title = barcode.product.title,
                Description = barcode.product.description,
                ImagePath = inventory.ImagePath,
                Categories = inventory.Categories
            };
        }

        private async Task<string> SaveImageAsync(Barcode barcode)
        {
            var imageStream = await imageLookup.Get(barcode.product.images[0]);
            var imagePath = $"/Images/{barcode.product.title}-{barcode.product.barcode}.jpeg";
            await imageRepository.Insert(imageStream, imagePath);
            return imagePath;
        }
    }
}