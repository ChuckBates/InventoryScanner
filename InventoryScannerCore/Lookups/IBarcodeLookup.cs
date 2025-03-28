using InventoryScannerCore.Models;

namespace InventoryScannerCore.Lookups
{
    public interface IBarcodeLookup
    {
        Task<Barcode> Get(string barcode);
    }
}