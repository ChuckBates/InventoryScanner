using System.Collections;

namespace InventoryScannerCore.Models
{
    public class Inventory
    {
        public Inventory(long barcode, string title, string description, int quantity, string imageurl)
        {
            Barcode = barcode;
            Title = title;
            Description = description;
            Quantity = quantity;
            ImageUrl = imageurl;

        }
        public long Barcode { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }
    }
}
