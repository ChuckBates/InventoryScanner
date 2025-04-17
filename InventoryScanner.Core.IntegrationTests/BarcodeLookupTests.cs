using InventoryScanner.Core.Lookups;
using InventoryScanner.Core.Models;
using InventoryScanner.Core.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryScanner.Core.IntegrationTests
{
    [TestFixture]
    public class BarcodeLookupTests
    {
        BarcodeLookup barcodeLookup;

        [OneTimeSetUp]
        public async Task SetupAsync()
        {
            var testHelper = new IntegrationTestDependencyHelper();
            await testHelper.SpinUp();

            var settingsService = testHelper.Provider.GetRequiredService<ISettingsService>();
            if (settingsService == null)
            {
                throw new Exception("Settings service is null.");
            }

            barcodeLookup = new BarcodeLookup(settingsService);
        }

        [Test]
        public async Task When_looking_up_a_barcode()
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

            var actual = await barcodeLookup.Get(barcode);

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.product.barcode, Is.EqualTo(expected.product.barcode));
            Assert.That(actual.product.title, Is.EqualTo(expected.product.title));
            Assert.That(actual.product.description, Is.EqualTo(expected.product.description));
            Assert.That(actual.product.images, Is.EqualTo(expected.product.images));
        }

        [Test]
        public async Task When_looking_up_a_barcode_and_multiple_results_are_returned()
        {
            var barcode = "0368";
            var expected = new Barcode
            {
                product = new BarcodeProduct
                {
                    barcode = barcode,
                    title = "Radium Engineering 14-0368 8AN ORB to 3/8-Inch SAE Male",
                    description = "",
                    images = ["https://images.barcodelookup.com/26092/260920470-1.jpg"]
                }
            };

            Barcode? actual = null;
            for (int i = 0; i < 5; i++)
            {
                actual = await barcodeLookup.Get(barcode);
                if (actual?.product != null)
                {
                    break;
                }
                await Task.Delay(1000);
            }

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.product, Is.Not.Null);
            Assert.That(actual.product.barcode, Is.EqualTo(expected.product.barcode));
            Assert.That(actual.product.title, Is.EqualTo(expected.product.title));
            Assert.That(actual.product.description, Is.EqualTo(expected.product.description));
            Assert.That(actual.product.images, Is.EqualTo(expected.product.images));
        }

        [Test]
        public async Task When_looking_up_a_null_barcode()
        {
            string? barcode = null;

            var actual = await barcodeLookup.Get(barcode);

            Assert.That(actual.product, Is.Null);
        }

        [Test]
        public async Task When_looking_up_a_barcode_that_does_not_exist()
        {
            var barcode = "0";

            var actual = await barcodeLookup.Get(barcode);

            Assert.That(actual.product, Is.Null);
        }
    }
}
