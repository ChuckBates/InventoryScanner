using InventoryScannerCore.Models;
using InventoryScannerCore.Repositories;

namespace InventoryScannerCore.IntegrationTests
{
    [TestFixture]
    public class InventoryRepositoryTests
    {
        private InventoryRepository repository;

        [SetUp]
        public void Setup()
        {
            var settingsService = IntegrationTestHelper.GetSettingsService();
            if (settingsService == null)
            {
                throw new Exception("Settings service is null.");
            }

            repository = new InventoryRepository(settingsService);
            repository.DeleteAll();
        }

        [Test]
        public void When_roud_tripping_an_inventory()
        {
            var expected = TestInventories().First();

            repository.Insert(expected);

            var actual = repository.Get(expected.Barcode);

            Assert.IsNotNull(actual);
            Assert.That(actual.Barcode, Is.EqualTo(expected.Barcode));
            Assert.That(actual.Title, Is.EqualTo(expected.Title));
            Assert.That(actual.Description, Is.EqualTo(expected.Description));
            Assert.That(actual.Quantity, Is.EqualTo(expected.Quantity));
            Assert.That(actual.ImagePath, Is.EqualTo(expected.ImagePath));
            Assert.That(actual.Categories, Is.EquivalentTo(expected.Categories));

            repository.Delete(actual.Barcode);
        }

        [Test]
        public void When_getting_an_inventory_and_it_does_not_exist()
        {
            var barcode = GenerateBarcode();

            var actual = repository.Get(barcode);

            Assert.IsNull(actual);
        }

        [Test]
        public void When_getting_all_inventory()
        {
            repository.DeleteAll();
            
            var testInventories = TestInventories();
            testInventories.ForEach(i => repository.Insert(i));

            var inventories = repository.GetAll();

            Assert.IsNotNull(inventories);
            Assert.That(inventories.Count(), Is.EqualTo(2));

            Assert.That(inventories.First().Barcode, Is.EqualTo(testInventories.First().Barcode));
            Assert.That(inventories.First().Title, Is.EqualTo(testInventories.First().Title));
            Assert.That(inventories.First().Description, Is.EqualTo(testInventories.First().Description));
            Assert.That(inventories.First().Quantity, Is.EqualTo(testInventories.First().Quantity));
            Assert.That(inventories.First().ImagePath, Is.EqualTo(testInventories.First().ImagePath));
            Assert.That(inventories.First().Categories, Is.EquivalentTo(testInventories.First().Categories));

            Assert.That(inventories.Last().Barcode, Is.EqualTo(testInventories.Last().Barcode));
            Assert.That(inventories.Last().Title, Is.EqualTo(testInventories.Last().Title));
            Assert.That(inventories.Last().Description, Is.EqualTo(testInventories.Last().Description));
            Assert.That(inventories.Last().Quantity, Is.EqualTo(testInventories.Last().Quantity));
            Assert.That(inventories.Last().ImagePath, Is.EqualTo(testInventories.Last().ImagePath));
            Assert.That(inventories.Last().Categories, Is.EquivalentTo(testInventories.Last().Categories));

            inventories.ToList().ForEach(i => repository.Delete(i.Barcode));
        }

        [Test]
        public void When_inserting_an_inventory_and_it_already_exists()
        {
            var inventory = TestInventories().First();
            repository.Insert(inventory);

            inventory.Title += "-updated";
            inventory.Description += "-updated";
            inventory.Quantity += 10;
            inventory.ImagePath += "-updated";

            repository.Insert(inventory);

            var result = repository.Get(inventory.Barcode);

            Assert.That(result.Title, Is.EqualTo(inventory.Title));
            Assert.That(result.Description, Is.EqualTo(inventory.Description));
            Assert.That(result.Quantity, Is.EqualTo(inventory.Quantity));
            Assert.That(result.ImagePath, Is.EqualTo(inventory.ImagePath));
            Assert.That(result.Categories, Is.EquivalentTo(inventory.Categories));

            repository.Delete(inventory.Barcode);
        }

        public static List<Inventory> TestInventories()
        {
            return new List<Inventory>
            {
                new Inventory(GenerateBarcode(), "title1", "description1", 324, "images/category/photo.png", ["first", "second"]),
                new Inventory(GenerateBarcode(), "title2", "description2", 102, "images/category/image.jpg", ["first", "second"])
            };
        }

        public static string GenerateBarcode()
        {
            return new Random().NextInt64(100000000000, 999999999999).ToString();
        }
    }
}