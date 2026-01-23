using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using PreciousMetalsManager.Models;

namespace PreciousMetalsManager.Services
{
    public class LocalStorageService
    {
        private readonly string _dbPath = "holdings.db";

        public LocalStorageService()
        {
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            if (!File.Exists(_dbPath))
            {
                using var connection = new SqliteConnection($"Data Source={_dbPath}");
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText =
                    @"CREATE TABLE IF NOT EXISTS Holdings (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        MetalType INTEGER,
                        Form TEXT,
                        Purity REAL,
                        Weight REAL,
                        Quantity INTEGER,
                        PurchasePrice REAL,
                        PurchaseDate TEXT
                    );";
                cmd.ExecuteNonQuery();
            }
        }

        public List<MetalHolding> LoadHoldings()
        {
            var holdings = new List<MetalHolding>();
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT MetalType, Form, Purity, Weight, Quantity, PurchasePrice, PurchaseDate FROM Holdings";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                holdings.Add(new MetalHolding
                {
                    MetalType = (MetalType)reader.GetInt32(0),
                    Form = reader.GetString(1),
                    Purity = reader.GetDecimal(2),
                    Weight = reader.GetDecimal(3),
                    Quantity = reader.GetInt32(4),
                    PurchasePrice = reader.GetDecimal(5),
                    PurchaseDate = DateTime.Parse(reader.GetString(6))
                });
            }
            return holdings;
        }

        public void AddHolding(MetalHolding holding)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText =
                @"INSERT INTO Holdings (MetalType, Form, Purity, Weight, Quantity, PurchasePrice, PurchaseDate)
                  VALUES (@type, @form, @purity, @weight, @quantity, @price, @date)";
            cmd.Parameters.AddWithValue("@type", (int)holding.MetalType);
            cmd.Parameters.AddWithValue("@form", holding.Form);
            cmd.Parameters.AddWithValue("@purity", holding.Purity);
            cmd.Parameters.AddWithValue("@weight", holding.Weight);
            cmd.Parameters.AddWithValue("@quantity", holding.Quantity);
            cmd.Parameters.AddWithValue("@price", holding.PurchasePrice);
            cmd.Parameters.AddWithValue("@date", holding.PurchaseDate.ToString("o"));
            cmd.ExecuteNonQuery();
        }

        public void UpdateHolding(MetalHolding holding, int id)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText =
                @"UPDATE Holdings SET
                    MetalType=@type, Form=@form, Purity=@purity, Weight=@weight, Quantity=@quantity, PurchasePrice=@price, PurchaseDate=@date
                  WHERE Id=@id";
            cmd.Parameters.AddWithValue("@type", (int)holding.MetalType);
            cmd.Parameters.AddWithValue("@form", holding.Form);
            cmd.Parameters.AddWithValue("@purity", holding.Purity);
            cmd.Parameters.AddWithValue("@weight", holding.Weight);
            cmd.Parameters.AddWithValue("@quantity", holding.Quantity);
            cmd.Parameters.AddWithValue("@price", holding.PurchasePrice);
            cmd.Parameters.AddWithValue("@date", holding.PurchaseDate.ToString("o"));
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        public void DeleteHolding(int id)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM Holdings WHERE Id=@id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }
}
