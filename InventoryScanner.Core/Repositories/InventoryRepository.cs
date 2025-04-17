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
        private readonly string connectionString;

        public InventoryRepository(ISettingsService settings)
        {
            connectionString = settings.GetPostgresConnectionString();
        }

        private NpgsqlConnection GetConnection() => new(connectionString);

        public async Task<IEnumerable<Inventory>> GetAll(DateTime since, int pageNumber, int pageSize)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            var result = new List<Inventory>();
            var offset = (pageNumber - 1) * pageSize;
            var overFetchLimit = pageSize + 1;
            var query = $"SELECT * FROM inventory " +
                $"WHERE updated_at >= @s " +
                $"ORDER BY updated_at ASC, barcode ASC " +
                $"OFFSET @o LIMIT @l;";
            NpgsqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("s", since);
            command.Parameters.AddWithValue("o", offset);
            command.Parameters.AddWithValue("l", overFetchLimit);

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
                        categories: (await reader.GetFieldValueAsync<string[]>("categories")).ToList(),
                        updatedAt: await reader.GetFieldValueAsync<DateTime>("updated_at")
                    )
                );
            }

            await connection.CloseAsync();
            return result;
        }

        public async Task<Inventory?> Get(string barcode)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            var query = $"SELECT * FROM INVENTORY WHERE barcode = @b";
            NpgsqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("b", barcode);

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
                categories: (await reader.GetFieldValueAsync<string[]>("categories")).ToList(),
                updatedAt: await reader.GetFieldValueAsync<DateTime>("updated_at")
            );

            await connection.CloseAsync();
            return result;
        }

        public async Task<int> Insert(Inventory inventory)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            var statement = $"" +
                "INSERT INTO inventory (barcode, title, description, quantity, image_path, categories, updated_at) " +
                "values (@b, @t, @d, @q, @i, @c, @u) " +
                "ON CONFLICT(barcode) " +
                "DO UPDATE SET title = @t, description = @d, quantity = @q, image_path = @i, categories = @c, updated_at = @u";
            NpgsqlCommand command = new(statement, connection);
            command.Parameters.AddWithValue("b", inventory.Barcode);
            command.Parameters.AddWithValue("t", inventory.Title);
            command.Parameters.AddWithValue("d", inventory.Description);
            command.Parameters.AddWithValue("q", inventory.Quantity);
            command.Parameters.AddWithValue("i", inventory.ImagePath);
            command.Parameters.AddWithValue("c", inventory.Categories.ToArray());
            command.Parameters.AddWithValue("u", DateTime.UtcNow);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            await connection.CloseAsync();

            return rowsAffected;
        }

        public async Task Delete(string barcode)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            var statement = $"DELETE FROM inventory WHERE barcode = @b";
            NpgsqlCommand command = new(statement, connection);
            command.Parameters.AddWithValue("b", barcode);

            await command.ExecuteNonQueryAsync();
            await connection.CloseAsync();
        }

        internal async Task DeleteAll()
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            var statement = $"DELETE FROM inventory";
            NpgsqlCommand command = new(statement, connection);
            await command.ExecuteNonQueryAsync();
            await connection.CloseAsync();
        }
    }
}
