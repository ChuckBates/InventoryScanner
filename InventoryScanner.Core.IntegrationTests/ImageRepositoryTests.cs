using InventoryScanner.Core.Repositories;

namespace InventoryScanner.Core.IntegrationTests
{
    [TestFixture]
    public class ImageRepositoryTests
    {
        private ImageRepository repository;
        private string testImagePath;
        private string savedImagePath;
        private byte[] testImageByteArray;
        private Stream testImageStream;

        [SetUp]
        public async Task Setup()
        {
            repository = new ImageRepository();
            testImagePath = Directory.GetCurrentDirectory() + "/TestImages/spam.png";
            savedImagePath = Directory.GetCurrentDirectory() + "/TestImages/spam-test.png";
            testImageByteArray = await File.ReadAllBytesAsync(testImagePath);
            testImageStream = new MemoryStream(testImageByteArray);
        }

        [Test]
        public async Task When_round_tripping_an_image_byte_array()
        {
            await repository.Insert(testImageStream, savedImagePath);

            var savedImageByteArray = await repository.Get(testImagePath);
                
            Assert.That(savedImageByteArray, Is.Not.Null);
            Assert.That(savedImageByteArray, Is.EqualTo(testImageByteArray));

            repository.Delete(savedImagePath);
        }

        [Test]
        public async Task When_getting_an_image_and_it_does_not_exist()
        {
            var image = await repository.Get("nonexistent.png");

            Assert.That(image, Is.Null);
        }

        [Test]
        public void When_inserting_an_image_and_the_path_does_not_exist()
        {
            Assert.That(async () => await repository.Insert(testImageStream, ""), Throws.Nothing);
            Assert.That(async () => await repository.Insert(testImageStream, ""), Does.Contain("The value cannot be an empty string"));
        }

        [Test]
        public async Task When_inserting_an_image_and_the_image_already_exists()
        {
            await repository.Insert(testImageStream, savedImagePath);

            Assert.That(async () => await repository.Insert(testImageStream, savedImagePath), Throws.Nothing);
            Assert.That(async () => await repository.Insert(testImageStream, savedImagePath), Does.Contain("success"));

            repository.Delete(savedImagePath);
        }

        [Test]
        public void  When_inserting_an_image_and_the_image_name_has_spaces()
        {
            var imagePath = Directory.GetCurrentDirectory() + "/TestImages/test image.png";
            var despacedImagePath = Directory.GetCurrentDirectory() + "/TestImages/testimage.png";

            Assert.That(async () => await repository.Insert(testImageStream, imagePath), Throws.Nothing);
            Assert.That(async () => await repository.Insert(testImageStream, imagePath), Does.Contain("success"));

            repository.Delete(despacedImagePath);
        }

        [Test]
        public void When_deleting_an_image_and_it_does_not_exist()
        {
            Assert.That(() => repository.Delete("/bad-directory/nonexistent.png"), Throws.Nothing);
            Assert.That(() => repository.Delete("/bad-directory/nonexistent.png"), Is.False);
        }
    }
}
