using InventoryScannerCore.Lookups;
using InventoryScannerCore.Settings;

namespace InventoryScannerCore.IntegrationTests
{
    [TestFixture]
    public class ImageLookupTests
    {
        ImageLookup imageLookup;

        [SetUp]
        public void Setup()
        {
            var settingsService = IntegrationTestHelper.GetSettingsService();
            if (settingsService == null)
            {
                throw new Exception("Settings service is null.");
            }

            imageLookup = new ImageLookup(settingsService, new HttpClient());
        }

        [Test]
        public async Task When_looking_up_an_image()
        {
            var imageUrl = "https://images.barcodelookup.com/9054/90542079-1.jpg";
            var imageAsStream = await imageLookup.Get(imageUrl);

            Assert.That(imageAsStream, Is.Not.Null);
            Assert.That(imageAsStream.CanRead, Is.True);
        }

        [Test]
        public async Task When_looking_up_an_image_that_does_not_exist()
        {
            var imageUrl = "https://images.barcodelookup.com/9054/not_real.jpg";
            var imageAsStream = await imageLookup.Get(imageUrl);

            Assert.That(imageAsStream, Is.Null);
        }

        [Test]
        public void When_looking_up_an_image_with_invalid_url_format()
        {
            var invalidUrl = "invalid_url";

            Assert.That(async () => await imageLookup.Get(invalidUrl), Throws.Nothing);
            Assert.That(async () => await imageLookup.Get(invalidUrl), Is.Null);
        }

        [Test]
        public async Task When_looking_up_an_image_with_empty_url()
        {
            var emptyUrl = string.Empty;

            Assert.That(async () => await imageLookup.Get(emptyUrl), Throws.Nothing);
            Assert.That(async () => await imageLookup.Get(emptyUrl), Is.Null);
        }

        [Test]
        public async Task When_looking_up_an_image_with_null_url()
        {
            string nullUrl = null;

            Assert.That(async () => await imageLookup.Get(nullUrl), Throws.Nothing);
            Assert.That(async () => await imageLookup.Get(nullUrl), Is.Null);
        }

        [Test]
        public async Task When_looking_up_an_image_with_slow_network_response()
        {
            var slowUrl = "https://httpstat.us/200?sleep=5000";
            var imageAsStream = await imageLookup.Get(slowUrl);

            Assert.That(imageAsStream, Is.Not.Null);
            Assert.That(imageAsStream.CanRead, Is.True);
        }

        [Test]
        public async Task When_looking_up_an_image_with_unauthorized_access()
        {
            var unauthorizedUrl = "https://httpstat.us/401";
            var imageAsStream = await imageLookup.Get(unauthorizedUrl);

            Assert.That(imageAsStream, Is.Null);
        }

        [Test]
        public async Task When_looking_up_an_image_with_server_error()
        {
            var serverErrorUrl = "https://httpstat.us/500";
            var imageAsStream = await imageLookup.Get(serverErrorUrl);

            Assert.That(imageAsStream, Is.Null);
        }

        [Test]
        public async Task When_looking_up_a_large_image()
        {
            var largeImageUrl = "https://sampletestfile.com/wp-content/uploads/2023/05/15.8-MB.bmp";
            var imageAsStream = await imageLookup.Get(largeImageUrl);

            Assert.That(imageAsStream, Is.Not.Null);
            Assert.That(imageAsStream.CanRead, Is.True);
        }
    }
}
