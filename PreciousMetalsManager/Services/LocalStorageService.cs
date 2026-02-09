using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using PreciousMetalsManager.Models;
using System.Windows;

namespace PreciousMetalsManager.Services
{
    public class LocalStorageService
    {
        private readonly string _dbPath;

        public LocalStorageService(string? dbPath = null)
        {
            _dbPath = dbPath ?? "holdings.db";
            InitializeDatabase();
        }

        private static string L(string key)
            => Application.Current?.TryFindResource(key) as string ?? key;

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
                        PurchaseDate TEXT,
                        CollectableType INTEGER DEFAULT 0
                    );";
                cmd.ExecuteNonQuery();
            }
        }

        public List<MetalHolding> LoadHoldings()
        {
            var holdings = new List<MetalHolding>();
            try
            {
                using var connection = new SqliteConnection($"Data Source={_dbPath}");
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT Id, MetalType, Form, Purity, Weight, Quantity, PurchasePrice, PurchaseDate, CollectableType FROM Holdings";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    try
                    {
                        holdings.Add(new MetalHolding
                        {
                            Id = reader.GetInt32(0),
                            MetalType = (MetalType)reader.GetInt32(1),
                            Form = reader.GetString(2),
                            Purity = reader.GetDecimal(3),
                            Weight = reader.GetDecimal(4),
                            Quantity = reader.GetInt32(5),
                            PurchasePrice = reader.GetDecimal(6),
                            PurchaseDate = DateTime.Parse(reader.GetString(7)),
                            CollectableType = reader.IsDBNull(8) ? CollectableType.Bullion : (CollectableType)reader.GetInt32(8)
                        });
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Corrupt record ignored: " + ex.Message);
                        MessageBox.Show(
                            $"{L("Db_Msg_CorruptRecordIgnored")}\n{ex.Message}",
                            L("Db_Title_Warning"),
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("DB error: " + ex.Message);
                LogError("DB error", ex);
                MessageBox.Show(
                    $"{L("Db_Msg_LoadError")}\n{ex.Message}",
                    L("Db_Title_Error"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            return holdings;
        }

        public void AddHolding(MetalHolding holding)
        {
            try
            {
                using var connection = new SqliteConnection($"Data Source={_dbPath}");
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO Holdings (MetalType, Form, Purity, Weight, Quantity, PurchasePrice, PurchaseDate, CollectableType)
                    VALUES (@MetalType, @Form, @Purity, @Weight, @Quantity, @PurchasePrice, @PurchaseDate, @CollectableType);";
                cmd.Parameters.AddWithValue("@MetalType", (int)holding.MetalType);
                cmd.Parameters.AddWithValue("@Form", holding.Form);
                cmd.Parameters.AddWithValue("@Purity", holding.Purity);
                cmd.Parameters.AddWithValue("@Weight", holding.Weight);
                cmd.Parameters.AddWithValue("@Quantity", holding.Quantity);
                cmd.Parameters.AddWithValue("@PurchasePrice", holding.PurchasePrice);
                cmd.Parameters.AddWithValue("@PurchaseDate", holding.PurchaseDate.ToString("o"));
                cmd.Parameters.AddWithValue("@CollectableType", (int)holding.CollectableType);

                cmd.ExecuteNonQuery();
                cmd.CommandText = "SELECT last_insert_rowid();";
                holding.Id = Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("DB error: " + ex.Message);
                LogError("DB error", ex);
                MessageBox.Show(
                    $"{L("Db_Msg_SaveError")}\n{ex.Message}",
                    L("Db_Title_Error"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public void UpdateHolding(MetalHolding holding, int id)
        {
            try
            {
                using var connection = new SqliteConnection($"Data Source={_dbPath}");
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText =
                    @"UPDATE Holdings SET
                        MetalType = @MetalType,
                        Form = @Form,
                        Purity = @Purity,
                        Weight = @Weight,
                        Quantity = @Quantity,
                        PurchasePrice = @PurchasePrice,
                        PurchaseDate = @PurchaseDate,
                        CollectableType = @CollectableType
                        WHERE Id = @Id";
                cmd.Parameters.AddWithValue("@MetalType", (int)holding.MetalType);
                cmd.Parameters.AddWithValue("@Form", holding.Form);
                cmd.Parameters.AddWithValue("@Purity", holding.Purity);
                cmd.Parameters.AddWithValue("@Weight", holding.Weight);
                cmd.Parameters.AddWithValue("@Quantity", holding.Quantity);
                cmd.Parameters.AddWithValue("@PurchasePrice", holding.PurchasePrice);
                cmd.Parameters.AddWithValue("@PurchaseDate", holding.PurchaseDate.ToString("o"));
                cmd.Parameters.AddWithValue("@CollectableType", (int)holding.CollectableType);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("DB error: " + ex.Message);
                LogError("DB error", ex);
                MessageBox.Show(
                    $"{L("Db_Msg_UpdateError")}\n{ex.Message}",
                    L("Db_Title_Error"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public void DeleteHolding(int id)
        {
            try
            {
                using var connection = new SqliteConnection($"Data Source={_dbPath}");
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = "DELETE FROM Holdings WHERE Id=@id";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("DB error: " + ex.Message);
                LogError("DB error", ex);
                MessageBox.Show(
                    $"{L("Db_Msg_DeleteError")}\n{ex.Message}",
                    L("Db_Title_Error"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void LogError(string message, Exception ex)
        {
            var logPath = "error.log";
            var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message} | {ex}\n";
            File.AppendAllText(logPath, logEntry);
        }
    }
}
