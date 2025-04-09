using System.Text;

namespace InventoryScanner.TestUtilities
{
    public static class Barcodes
    {
        public static string Generate()
        {
            var random = new Random();
            var barcode = new StringBuilder();

            for (int i = 0; i < 12; i++)
            {
                barcode.Append(random.Next(0, 10));
            }

            return barcode.ToString();
        }
    }
}
