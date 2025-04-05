using InventoryScanner.Core.Models;

namespace InventoryScanner.Core.Lookups
{
    public interface IBarcodeLookup
    {
        Task<Barcode> Get(string barcode);
    }
}