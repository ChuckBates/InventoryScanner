namespace InventoryScannerCore.Models
{
    public class Barcode
    {
        public BarcodeProduct product { get; set; }
    }

    public class BarcodeProduct
    {
        public string barcode { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string[] images { get; set; }
    }
}
