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

        public InventoryRepository()
        {
            string connectionString = "Server=localhost;Port=5432;Database=inventoryscanner;User Id=postgres;Password=postgres;";
            connection = new NpgsqlConnection(connectionString);
        }

        public IEnumerable<Inventory> GetAll()
        {
            connection.Open();
            var result = new List<Inventory>();
            var query = "SELECT * FROM INVENTORY;";
            NpgsqlCommand command = new NpgsqlCommand(query, connection);
            NpgsqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(
                    new Inventory(
                        (long)reader.GetValue("barcode"),
                        (string)reader.GetValue("title"),
                        (string)reader.GetValue("description"),
                        (int)reader.GetValue("quantity"),
                        (string)reader.GetValue("imageurl")
                    )
                );
            }

            connection.Close();
            return result;
        }

        public Inventory? Get(long barcode)
        {
            connection.Open();
            var query = $"SELECT * FROM INVENTORY WHERE barcode = {barcode}";
            NpgsqlCommand command = new NpgsqlCommand(query, connection);
            NpgsqlDataReader reader = command.ExecuteReader();
            if (!reader.HasRows)
            {
                connection.Close();
                return null;
            }

            reader.Read();
            var result = new Inventory(
                    (long)reader.GetValue("barcode"),
                    (string)reader.GetValue("title"),
                    (string)reader.GetValue("description"),
                    (int)reader.GetValue("quantity"),
                    (string)reader.GetValue("imageurl")
            );

            connection.Close();
            return result;
        }

        public void Insert(Inventory inventory)
        {
            connection.Open();
            var statement = $"INSERT INTO inventory (barcode, title, description, quantity, imageurl) values " +
                $"({inventory.Barcode}, '{inventory.Title}', '{inventory.Description}', {inventory.Quantity}, '{inventory.ImageUrl}')";
            NpgsqlCommand command = new NpgsqlCommand(statement, connection);
            command.ExecuteNonQuery();
            connection.Close();
        }

        public void Delete(long barcode)
        {
            connection.Open();
            var statement = $"DELETE FROM inventory WHERE barcode = {barcode}";
            NpgsqlCommand command = new NpgsqlCommand(statement, connection);
            command.ExecuteNonQuery();
            connection.Close();
        }

        internal void DeleteAll()
        {
            connection.Open();
            var statement = $"DELETE FROM inventory";
            NpgsqlCommand command = new NpgsqlCommand(statement, connection);
            command.ExecuteNonQuery();
            connection.Close();
        }
    }
}
