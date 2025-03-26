using InventoryScannerCore.Models;

namespace InventoryScannerCore.IntegrationTests
{
    [TestFixture]
    public class BarcodeLookupTests
    {
        BarcodeLookup barcodeLookup;

        [OneTimeSetUp]
        public void Setup()
        {
            var settingsService = IntegrationTestHelper.GetSettingsService();
            if (settingsService == null)
            {
                throw new Exception("Settings service is null.");
            }

            barcodeLookup = new BarcodeLookup(settingsService, new HttpClient());
        }

        [Test]
        public void When_looking_up_a_barcode()
        {
            var barcode = "0036800902176";
            var expected = new Barcode
            {
                product = new BarcodeProduct
                {
                    barcode = barcode,
                    title = "Homogenized Evaporated Milk",
                    description = "Homogenized Evaporated Milk. Serving size: 2 Tbsp (30 ml). Country of origin: United States.",
                    images = new string[] { "https://images.barcodelookup.com/9054/90542079-1.jpg" }
                }
            };

            var actual = barcodeLookup.Get(barcode).Result;

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.product.barcode, Is.EqualTo(expected.product.barcode));
            Assert.That(actual.product.title, Is.EqualTo(expected.product.title));
            Assert.That(actual.product.description, Is.EqualTo(expected.product.description));
            Assert.That(actual.product.images, Is.EqualTo(expected.product.images));
        }

        [Test]
        public void When_looking_up_a_barcode_that_does_not_exist()
        {
            var barcode = "0";

            var actual = barcodeLookup.Get(barcode).Result;

            Assert.That(actual.product, Is.Null);
        }
    }
}
