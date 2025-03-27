namespace InventoryScannerCore.Models
{
    public class Inventory
    {
        public Inventory()
        {
            Barcode = "";
            Title = "";
            Description = "";
            Quantity = 0;
            ImagePath = "";
        }

        public Inventory(string barcode, string title, string description, int quantity, string imagePath)
        {
            Barcode = barcode;
            Title = title;
            Description = description;
            Quantity = quantity;
            ImagePath = imagePath;

        }

        public string Barcode { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public string ImagePath { get; set; }
    }
}
