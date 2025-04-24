using InventoryScanner.Core.Models;

namespace InventoryScanner.Core.Wrappers
{
    public interface IBarcodeWrapper
    {
        Task<Barcode> Get(string barcode);
    }
}