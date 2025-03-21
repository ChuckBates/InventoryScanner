using NUnit.Framework;
using InventoryScannerCore.Models;
using InventoryScannerCore.Repositories;
using Assert = NUnit.Framework.Assert;

namespace InventoryScannerCore.IntegrationTests
{
    [TestClass]
    public class InventoryRepositoryTests
    {
        private InventoryRepository repository = new InventoryRepository();

        [TestMethod]
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
            Assert.That(actual.ImageUrl, Is.EqualTo(expected.ImageUrl));

            repository.Delete(actual.Barcode);
        }

        [TestMethod]
        public void When_getting_an_inventory_and_it_does_not_exist()
        {
            var barcode = GenerateBarcode();

            var actual = repository.Get(barcode);

            Assert.IsNull(actual);
        }

        [TestMethod]
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
            Assert.That(inventories.First().ImageUrl, Is.EqualTo(testInventories.First().ImageUrl));

            Assert.That(inventories.Last().Barcode, Is.EqualTo(testInventories.Last().Barcode));
            Assert.That(inventories.Last().Title, Is.EqualTo(testInventories.Last().Title));
            Assert.That(inventories.Last().Description, Is.EqualTo(testInventories.Last().Description));
            Assert.That(inventories.Last().Quantity, Is.EqualTo(testInventories.Last().Quantity));
            Assert.That(inventories.Last().ImageUrl, Is.EqualTo(testInventories.Last().ImageUrl));

            inventories.ToList().ForEach(i => repository.Delete(i.Barcode));
        }

        public static List<Inventory> TestInventories()
        {
            return new List<Inventory>
            {
                new Inventory(GenerateBarcode(), "title1", "description1", 324, "image.url.com/img/23"),
                new Inventory(GenerateBarcode(), "title2", "description2", 102, "image.url.com/img/3")
            };
        }

        public static long GenerateBarcode()
        {
            return new Random().NextInt64(100000000000, 999999999999);
        }
    }
}