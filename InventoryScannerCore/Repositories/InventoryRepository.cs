using InventoryScannerCore.Models;
using Npgsql;
using System.Data;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("InventoryScannerCore.IntegrationTests")]

namespace InventoryScannerCore.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        NpgsqlConnection connection;

        public InventoryRepository(ISettingsService settings)
        {
            connection = new NpgsqlConnection((string?)settings.GetPostgresConnectionString());
        }

        public IEnumerable<Inventory> GetAll()
        {
            connection.Open();
            var result = new List<Inventory>();
            var query = "SELECT * FROM INVENTORY;";
            NpgsqlCommand command = new(query, connection);
            NpgsqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(
                    new Inventory(
                        barcode: (string)reader.GetValue("barcode"),
                        title: (string)reader.GetValue("title"),
                        description: (string)reader.GetValue("description"),
                        quantity: (int)reader.GetValue("quantity"),
                        imagePath: (string)reader.GetValue("image_path"),
                        categories: (string[])reader.GetValue("categories")
                    )
                );
            }

            connection.Close();
            return result;
        }

        public Inventory? Get(string barcode)
        {
            connection.Open();
            var query = $"SELECT * FROM INVENTORY WHERE barcode = '{barcode}'";
            NpgsqlCommand command = new(query, connection);
            NpgsqlDataReader reader = command.ExecuteReader();
            if (!reader.HasRows)
            {
                connection.Close();
                return null;
            }

            reader.Read();
            var result = new Inventory(
                    barcode: (string)reader.GetValue("barcode"),
                    title: (string)reader.GetValue("title"),
                    description: (string)reader.GetValue("description"),
                    quantity: (int)reader.GetValue("quantity"),
                    imagePath: (string)reader.GetValue("image_path"),
                    categories: (string[])reader.GetValue("categories")
            );

            connection.Close();
            return result;
        }

        public void Insert(Inventory inventory)
        {
            connection.Open();
            var parsedCategories = "{" + string.Join(",", inventory.Categories) + "}";
            var statement = $"" +
                $"INSERT INTO inventory (barcode, title, description, quantity, image_path, categories) " +
                $"values ('{inventory.Barcode}', '{inventory.Title}', '{inventory.Description}', {inventory.Quantity}, '{inventory.ImagePath}', '{parsedCategories}') " +
                $"ON CONFLICT(barcode) " +
                $"DO UPDATE SET title = '{inventory.Title}', description = '{inventory.Description}', quantity = {inventory.Quantity}, image_path = '{inventory.ImagePath}', categories = '{parsedCategories}'";
            NpgsqlCommand command = new(statement, connection);
            command.ExecuteNonQuery();
            connection.Close();
        }

        public void Delete(string barcode)
        {
            connection.Open();
            var statement = $"DELETE FROM inventory WHERE barcode = '{barcode}'";
            NpgsqlCommand command = new(statement, connection);
            command.ExecuteNonQuery();
            connection.Close();
        }

        internal void DeleteAll()
        {
            connection.Open();
            var statement = $"DELETE FROM inventory";
            NpgsqlCommand command = new(statement, connection);
            command.ExecuteNonQuery();
            connection.Close();
        }
    }
}
