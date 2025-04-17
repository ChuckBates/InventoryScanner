namespace InventoryScanner.Core.Models
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
            Categories = [];
            UpdatedAt = DateTime.UtcNow;
        }

        public Inventory(string barcode, string title, string description, int quantity, string imagePath, List<string> categories, DateTime updatedAt)
        {
            Barcode = barcode;
            Title = title;
            Description = description;
            Quantity = quantity;
            ImagePath = imagePath;
            Categories = categories;
            UpdatedAt = updatedAt;
        }

        public string Barcode { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public string ImagePath { get; set; }
        public List<string> Categories { get; set; }
        public DateTime UpdatedAt { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is Inventory inventory &&
                   Barcode == inventory.Barcode &&
                   Title == inventory.Title &&
                   Description == inventory.Description &&
                   Quantity == inventory.Quantity &&
                   ImagePath == inventory.ImagePath &&
                   Categories.SequenceEqual(inventory.Categories) &&
                   (UpdatedAt - inventory.UpdatedAt).Duration() < TimeSpan.FromSeconds(1);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Barcode, Title, Description, Quantity, ImagePath, Categories, UpdatedAt);
        }
    }
}
