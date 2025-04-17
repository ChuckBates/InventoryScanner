using InventoryScanner.Core.Models;
using InventoryScanner.Core.Repositories;
using InventoryScanner.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace InventoryScanner.Core.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public class InventoryRepositoryTests
    {
        private InventoryRepository repository;

        [SetUp]
        public async Task Setup()
        {
            var testHelper = new IntegrationTestDependencyHelper();
            await testHelper.SpinUp();

            var settingsService = testHelper.Provider.GetRequiredService<ISettingsService>();
            if (settingsService == null)
            {
                throw new Exception("Settings service is null.");
            }

            repository = new InventoryRepository(settingsService);
            await repository.DeleteAll();
        }

        [Test]
        public async Task When_roud_tripping_an_inventory()
        {
            var expected = (await TestInventories()).First();

            await repository.Insert(expected);

            var actual = await repository.Get(expected.Barcode);

            Assert.IsNotNull(actual);
            Assert.That(actual.Barcode, Is.EqualTo(expected.Barcode));
            Assert.That(actual.Title, Is.EqualTo(expected.Title));
            Assert.That(actual.Description, Is.EqualTo(expected.Description));
            Assert.That(actual.Quantity, Is.EqualTo(expected.Quantity));
            Assert.That(actual.ImagePath, Is.EqualTo(expected.ImagePath));
            Assert.That(actual.Categories, Is.EquivalentTo(expected.Categories));
            Assert.That(actual.UpdatedAt, Is.EqualTo(expected.UpdatedAt).Within(TimeSpan.FromMilliseconds(1)));

            await repository.Delete(actual.Barcode);
        }

        [Test]
        public async Task When_getting_an_inventory_and_it_does_not_exist()
        {
            var barcode = GenerateBarcode();

            var actual = await repository.Get(barcode);

            Assert.IsNull(actual);
        }

        [Test]
        public async Task When_getting_all_inventory_with_no_filtering()
        {
            await repository.DeleteAll();
            
            var testInventories = await TestInventories();
            var totalRowsAffected = 0;
            foreach (var inventory in testInventories)
            {
                var rowsaffected = await repository.Insert(inventory);
                totalRowsAffected += rowsaffected;
            }

            var inventories = await repository.GetAll(DateTime.MinValue, 1, 50);

            Assert.IsNotNull(inventories);
            Assert.That(inventories.Count(), Is.EqualTo(10));
            Assert.That(totalRowsAffected, Is.EqualTo(testInventories.Count()));

            for (int i = 0; i < inventories.Count(); i++)
            {
                var testInventory = testInventories[i];
                var savedInventory = inventories.ElementAt(i);

                Assert.That(savedInventory.Barcode, Is.EqualTo(testInventory.Barcode));
                Assert.That(savedInventory.Title, Is.EqualTo(testInventory.Title));
                Assert.That(savedInventory.Description, Is.EqualTo(testInventory.Description));
                Assert.That(savedInventory.Quantity, Is.EqualTo(testInventory.Quantity));
                Assert.That(savedInventory.ImagePath, Is.EqualTo(testInventory.ImagePath));
                Assert.That(savedInventory.Categories, Is.EquivalentTo(testInventory.Categories));
                Assert.That(savedInventory.UpdatedAt, Is.EqualTo(testInventory.UpdatedAt).Within(TimeSpan.FromSeconds(1)));
            }

            await Task.WhenAll(inventories.Select(i => repository.Delete(i.Barcode)));
        }

        [Test]
        public async Task When_getting_all_inventory_with_filtering_getting_first_page()
        {
            await repository.DeleteAll();

            var since = DateTime.UtcNow.AddDays(-1);
            var pageNumber = 1;
            var pageSize = 5;
            var overPageSize = pageSize + 1;
            var testInventories = await TestInventories();
            var totalRowsAffected = 0;
            foreach (var inventory in testInventories)
            {
                var rowsaffected = await repository.Insert(inventory);
                totalRowsAffected += rowsaffected;
            }

            var inventories = await repository.GetAll(since, pageNumber, pageSize);

            Assert.IsNotNull(inventories);
            Assert.That(inventories.Count(), Is.EqualTo(overPageSize));
            Assert.That(totalRowsAffected, Is.EqualTo(testInventories.Count()));

            testInventories = testInventories
                .OrderBy(i => i.UpdatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(overPageSize)
                .ToList();

            for (int i = 0; i < inventories.Count(); i++)
            {
                var testInventory = testInventories[i];
                var savedInventory = inventories.ElementAt(i);

                Assert.That(savedInventory.Barcode, Is.EqualTo(testInventory.Barcode));
                Assert.That(savedInventory.Title, Is.EqualTo(testInventory.Title));
                Assert.That(savedInventory.Description, Is.EqualTo(testInventory.Description));
                Assert.That(savedInventory.Quantity, Is.EqualTo(testInventory.Quantity));
                Assert.That(savedInventory.ImagePath, Is.EqualTo(testInventory.ImagePath));
                Assert.That(savedInventory.Categories, Is.EquivalentTo(testInventory.Categories));
                Assert.That(savedInventory.UpdatedAt, Is.EqualTo(testInventory.UpdatedAt).Within(TimeSpan.FromSeconds(1)));
            }

            await Task.WhenAll(inventories.Select(i => repository.Delete(i.Barcode)));
        }

        [Test]
        public async Task When_getting_all_inventory_with_filtering_getting_second_page()
        {
            await repository.DeleteAll();

            var since = DateTime.UtcNow.AddDays(-1);
            var pageNumber = 2;
            var pageSize = 5;
            var testInventories = await TestInventories();
            var totalRowsAffected = 0;
            foreach (var inventory in testInventories)
            {
                var rowsaffected = await repository.Insert(inventory);
                totalRowsAffected += rowsaffected;
            }

            var inventories = await repository.GetAll(since, pageNumber, pageSize);

            Assert.IsNotNull(inventories);
            Assert.That(inventories.Count(), Is.EqualTo(pageSize));
            Assert.That(totalRowsAffected, Is.EqualTo(testInventories.Count()));

            testInventories = testInventories
                .OrderBy(i => i.UpdatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            for (int i = 0; i < inventories.Count(); i++)
            {
                var testInventory = testInventories[i];
                var savedInventory = inventories.ElementAt(i);

                Assert.That(savedInventory.Barcode, Is.EqualTo(testInventory.Barcode));
                Assert.That(savedInventory.Title, Is.EqualTo(testInventory.Title));
                Assert.That(savedInventory.Description, Is.EqualTo(testInventory.Description));
                Assert.That(savedInventory.Quantity, Is.EqualTo(testInventory.Quantity));
                Assert.That(savedInventory.ImagePath, Is.EqualTo(testInventory.ImagePath));
                Assert.That(savedInventory.Categories, Is.EquivalentTo(testInventory.Categories));
                Assert.That(savedInventory.UpdatedAt, Is.EqualTo(testInventory.UpdatedAt).Within(TimeSpan.FromSeconds(1)));
            }

            await Task.WhenAll(inventories.Select(i => repository.Delete(i.Barcode)));
        }

        [Test]
        public async Task When_getting_all_inventory_with_filtering_getting_the_most_recent()
        {
            await repository.DeleteAll();

            var pageNumber = 1;
            var pageSize = 15;
            var totalRowsAffected = 0;
            var testInventoriesOld = await TestInventories(5);
            foreach (var inventory in testInventoriesOld)
            {
                var rowsaffected = await repository.Insert(inventory);
                totalRowsAffected += rowsaffected;
            }
            var since = DateTime.UtcNow;
            var testInventories = await TestInventories(5);
            foreach (var inventory in testInventories)
            {
                var rowsaffected = await repository.Insert(inventory);
                totalRowsAffected += rowsaffected;
            }

            testInventories.AddRange(testInventoriesOld);

            var inventories = await repository.GetAll(since, pageNumber, pageSize);

            Assert.IsNotNull(inventories);
            Assert.That(inventories.Count(), Is.EqualTo(5));
            Assert.That(totalRowsAffected, Is.EqualTo(testInventories.Count()));

            var expected = testInventories
                .Where(i => i.UpdatedAt >= since)
                .OrderBy(i => i.UpdatedAt)
                .ThenBy(i => i.Barcode)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            for (int i = 0; i < inventories.Count(); i++)
            {
                var expectedInventory = expected[i];
                var savedInventory = inventories.ElementAt(i);

                Assert.That(savedInventory.Barcode, Is.EqualTo(expectedInventory.Barcode));
                Assert.That(savedInventory.Title, Is.EqualTo(expectedInventory.Title));
                Assert.That(savedInventory.Description, Is.EqualTo(expectedInventory.Description));
                Assert.That(savedInventory.Quantity, Is.EqualTo(expectedInventory.Quantity));
                Assert.That(savedInventory.ImagePath, Is.EqualTo(expectedInventory.ImagePath));
                Assert.That(savedInventory.Categories, Is.EquivalentTo(expectedInventory.Categories));
                Assert.That(savedInventory.UpdatedAt, Is.EqualTo(expectedInventory.UpdatedAt).Within(TimeSpan.FromSeconds(1)));
            }

            await Task.WhenAll(inventories.Select(i => repository.Delete(i.Barcode)));
        }

        [Test]
        public async Task When_inserting_an_inventory_and_it_already_exists()
        {
            var inventory = (await TestInventories()).First();
            await repository.Insert(inventory);

            inventory.Title += "-updated";
            inventory.Description += "-updated";
            inventory.Quantity += 10;
            inventory.ImagePath += "-updated";

            await repository.Insert(inventory);

            var result = await repository.Get(inventory.Barcode);

            Assert.That(result.Title, Is.EqualTo(inventory.Title));
            Assert.That(result.Description, Is.EqualTo(inventory.Description));
            Assert.That(result.Quantity, Is.EqualTo(inventory.Quantity));
            Assert.That(result.ImagePath, Is.EqualTo(inventory.ImagePath));
            Assert.That(result.Categories, Is.EquivalentTo(inventory.Categories));

            await repository.Delete(inventory.Barcode);
        }

        public static async Task<List<Inventory>> TestInventories(int total = 10)
        {
            var inventories = new List<Inventory>();
            var title = "title";
            var description = "description";
            var baseUpdatedAt = DateTime.UtcNow;

            for (int i = 0; i < total; i++)
            {
                inventories.Add(new Inventory
                {
                    Barcode = GenerateBarcode(),
                    Title = title + i,
                    Description = description + i,
                    Quantity = i,
                    ImagePath = "images/category/image" + i + ".png",
                    Categories = new List<string> { "first", "second" },
                    UpdatedAt = baseUpdatedAt.AddMilliseconds(i)
                }); 
            }

            return inventories;
        }

        public static string GenerateBarcode()
        {
            return new Random().NextInt64(100000000000, 999999999999).ToString();
        }
    }
}