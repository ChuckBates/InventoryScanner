using InventoryScannerCore.Repositories;
using System.Drawing;

namespace InventoryScannerCore.IntegrationTests
{
    [TestFixture]
    public class ImageRepositoryTests
    {
        private ImageRepository repository;
        private string testImagePath;
        private string savedImagePath;

        [SetUp]
        public void Setup()
        {
            repository = new ImageRepository();
            testImagePath = Directory.GetCurrentDirectory() + "/TestImages/spam.png";
            savedImagePath = Directory.GetCurrentDirectory() + "/TestImages/spam-test.png";
        }

        [Test]
        public void When_round_tripping_an_image()
        {
            using (Image testImage = Image.FromStream(new MemoryStream(File.ReadAllBytes(testImagePath))))
            {
                repository.Insert(testImage, savedImagePath);

                var savedImage = repository.Get(testImagePath);
                
                Assert.That(savedImage, Is.Not.Null);
                Assert.That(ImageToByteArray(savedImage), Is.EqualTo(ImageToByteArray(testImage)));
            }

            repository.Delete(savedImagePath);
        }

        [Test]
        public void When_getting_an_image_and_it_does_not_exist()
        {
            var image = repository.Get("nonexistent.png");

            Assert.That(image, Is.Null);
        }

        [Test]
        public void When_inserting_an_image_and_the_path_does_not_exist()
        {
            using (Image testImage = Image.FromStream(new MemoryStream(File.ReadAllBytes(testImagePath))))
            {
                Assert.That(() => repository.Insert(testImage, ""), Throws.Nothing);
                Assert.That(() => repository.Insert(testImage, ""), Is.False);
            }
        }

        [Test]
        public void When_inserting_an_image_and_the_image_already_exists()
        {
            using (Image testImage = Image.FromStream(new MemoryStream(File.ReadAllBytes(testImagePath))))
            {
                repository.Insert(testImage, savedImagePath);

                Assert.That(() => repository.Insert(testImage, savedImagePath), Throws.Nothing);
                Assert.That(() => repository.Insert(testImage, savedImagePath), Is.True);
            }

            repository.Delete(savedImagePath);
        }

        [Test]
        public void When_deleting_an_image_and_it_does_not_exist()
        {
            Assert.That(() => repository.Delete("/bad-directory/nonexistent.png"), Throws.Nothing);
            Assert.That(() => repository.Delete("/bad-directory/nonexistent.png"), Is.False);
        }

        private byte[] ImageToByteArray(Image image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, image.RawFormat);
                return ms.ToArray();
            }
        }
    }
}
