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
    }
}
