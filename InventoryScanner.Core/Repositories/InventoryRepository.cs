using InventoryScanner.Core.Models;
using InventoryScanner.Core.Settings;
using Npgsql;
using System.Data;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("InventoryScanner.Core.IntegrationTests")]

namespace InventoryScanner.Core.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        NpgsqlConnection connection;

        public InventoryRepository(ISettingsService settings)
        {
            connection = new NpgsqlConnection((string?)settings.GetPostgresConnectionString());
        }

        public async Task<IEnumerable<Inventory>> GetAll()
        {
            await connection.OpenAsync();
            var result = new List<Inventory>();
            var query = "SELECT * FROM INVENTORY;";
            NpgsqlCommand command = new(query, connection);
            NpgsqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(
                    new Inventory(
                        barcode: await reader.GetFieldValueAsync<string>("barcode"),
                        title: await reader.GetFieldValueAsync<string>("title"),
                        description: await reader.GetFieldValueAsync<string>("description"),
                        quantity: await reader.GetFieldValueAsync<int>("quantity"),
                        imagePath: await reader.GetFieldValueAsync<string>("image_path"),
                        categories: (await reader.GetFieldValueAsync<string[]>("categories")).ToList()
                    )
                );
            }

            await connection.CloseAsync();
            return result;
        }

        public async Task<Inventory?> Get(string barcode)
        {
            await connection.OpenAsync();
            var query = $"SELECT * FROM INVENTORY WHERE barcode = '{barcode}'";
            NpgsqlCommand command = new(query, connection);
            NpgsqlDataReader reader = await command.ExecuteReaderAsync();
            if (!reader.HasRows)
            {
                await connection.CloseAsync();
                return null;
            }

            await reader.ReadAsync();
            var result = new Inventory(
                barcode: await reader.GetFieldValueAsync<string>("barcode"),
                title: await reader.GetFieldValueAsync<string>("title"),
                description: await reader.GetFieldValueAsync<string>("description"),
                quantity: await reader.GetFieldValueAsync<int>("quantity"),
                imagePath: await reader.GetFieldValueAsync<string>("image_path"),
                categories: (await reader.GetFieldValueAsync<string[]>("categories")).ToList()
            );

            await connection.CloseAsync();
            return result;
        }

        public async Task<int> Insert(Inventory inventory)
        {
            await connection.OpenAsync();
            var parsedCategories = "{" + string.Join(",", inventory.Categories) + "}";
            var statement = $"" +
                $"INSERT INTO inventory (barcode, title, description, quantity, image_path, categories) " +
                $"values ('{inventory.Barcode}', '{inventory.Title}', '{inventory.Description}', {inventory.Quantity}, '{inventory.ImagePath}', '{parsedCategories}') " +
                $"ON CONFLICT(barcode) " +
                $"DO UPDATE SET title = '{inventory.Title}', description = '{inventory.Description}', quantity = {inventory.Quantity}, image_path = '{inventory.ImagePath}', categories = '{parsedCategories}'";
            NpgsqlCommand command = new(statement, connection);
            var rowsAffected = await command.ExecuteNonQueryAsync();
            await connection.CloseAsync();

            return rowsAffected;
        }

        public async Task Delete(string barcode)
        {
            await connection.OpenAsync();
            var statement = $"DELETE FROM inventory WHERE barcode = '{barcode}'";
            NpgsqlCommand command = new(statement, connection);
            await command.ExecuteNonQueryAsync();
            await connection.CloseAsync();
        }

        internal async Task DeleteAll()
        {
            await connection.OpenAsync();
            var statement = $"DELETE FROM inventory";
            NpgsqlCommand command = new(statement, connection);
            await command.ExecuteNonQueryAsync();
            await connection.CloseAsync();
        }
    }
}
