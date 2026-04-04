using Microsoft.Data.Sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PreciousMetalsManager.Models;
using PreciousMetalsManager.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PreciousMetalsManager.Tests
{
    [TestClass]
    public class LocalStorageServiceTest
    {
        private string _testDbPath = null!;
        private LocalStorageService _service = null!;
        private RecordingMessageService _messageService = null!;
        private StubTextProvider _textProvider = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _testDbPath = Path.Combine(Path.GetTempPath(), $"test_holdings_{Guid.NewGuid()}.db");
            _messageService = new RecordingMessageService();
            _textProvider = new StubTextProvider();
            _service = new LocalStorageService(_testDbPath, _messageService, _textProvider);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _service = null!;
            GC.Collect();
            GC.WaitForPendingFinalizers();

            if (File.Exists(_testDbPath))
            {
                try
                {
                    File.Delete(_testDbPath);
                }
                catch (IOException ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error deleting test database: " + ex.Message);
                }
            }
        }

        [TestMethod]
        public void AddHolding_ShouldPersistAndLoadHolding()
        {
            var holding = CreateTestHolding();

            _service.AddHolding(holding);
            var holdings = _service.LoadHoldings();

            Assert.IsTrue(holdings.Any(h => h.Form == holding.Form && h.MetalType == holding.MetalType));
            Assert.IsEmpty(_messageService.ErrorMessages);
        }

        [TestMethod]
        public void UpdateHolding_ShouldModifyExistingHolding()
        {
            var holding = CreateTestHolding();
            _service.AddHolding(holding);

            holding.Form = "UpdatedForm";
            _service.UpdateHolding(holding, holding.Id);
            var holdings = _service.LoadHoldings();

            Assert.IsTrue(holdings.Any(h => h.Id == holding.Id && h.Form == "UpdatedForm"));
            Assert.IsEmpty(_messageService.ErrorMessages);
        }

        [TestMethod]
        public void DeleteHolding_ShouldRemoveHolding()
        {
            var holding = CreateTestHolding();
            _service.AddHolding(holding);

            var holdingsBeforeDelete = _service.LoadHoldings();
            Assert.IsTrue(holdingsBeforeDelete.Any(h => h.Id == holding.Id));

            _service.DeleteHolding(holding.Id);
            var holdingsAfterDelete = _service.LoadHoldings();

            Assert.IsFalse(holdingsAfterDelete.Any(h => h.Id == holding.Id));
            Assert.IsEmpty(_messageService.ErrorMessages);
        }

        [TestMethod]
        public void LoadHoldings_ShouldReturnList()
        {
            var holdings = _service.LoadHoldings();

            Assert.IsNotNull(holdings);
            Assert.IsInstanceOfType(holdings, typeof(List<MetalHolding>));
            Assert.IsEmpty(_messageService.ErrorMessages);
        }

        [TestMethod]
        public void AddAndLoadHolding_ShouldPersistCollectableType()
        {
            foreach (CollectableType type in Enum.GetValues(typeof(CollectableType)))
            {
                var holding = new MetalHolding
                {
                    MetalType = MetalType.Gold,
                    Form = "Test",
                    Purity = 999.9m,
                    Weight = 1m,
                    Quantity = 1,
                    PurchasePrice = 100m,
                    PurchaseDate = DateTime.Today,
                    CollectableType = type
                };

                _service.AddHolding(holding);
                var loaded = _service.LoadHoldings().FirstOrDefault(h => h.Id == holding.Id);

                Assert.IsNotNull(loaded);
                Assert.AreEqual(type, loaded.CollectableType);
            }

            Assert.IsEmpty(_messageService.ErrorMessages);
        }

        [TestMethod]
        public void UpdateHolding_ShouldChangeCollectableType()
        {
            var holding = new MetalHolding
            {
                MetalType = MetalType.Gold,
                Form = "Test",
                Purity = 999.9m,
                Weight = 1m,
                Quantity = 1,
                PurchasePrice = 100m,
                PurchaseDate = DateTime.Today,
                CollectableType = CollectableType.Bullion
            };

            _service.AddHolding(holding);
            holding.CollectableType = CollectableType.Numismatic;
            _service.UpdateHolding(holding, holding.Id);

            var loaded = _service.LoadHoldings().FirstOrDefault(h => h.Id == holding.Id);
            Assert.IsNotNull(loaded);
            Assert.AreEqual(CollectableType.Numismatic, loaded.CollectableType);
            Assert.IsEmpty(_messageService.ErrorMessages);
        }

        [TestMethod]
        public void AddHolding_ShouldSetInsertedId()
        {
            var holding = CreateTestHolding();

            _service.AddHolding(holding);

            Assert.IsGreaterThan(holding.Id, 0);
            Assert.IsEmpty(_messageService.ErrorMessages);
        }

        [TestMethod]
        public void LoadHoldings_ShouldDefaultCollectableTypeToBullion_WhenDatabaseValueIsNull()
        {
            InsertHoldingRowWithNullCollectableType();

            var loaded = _service.LoadHoldings().Single();

            Assert.AreEqual(CollectableType.Bullion, loaded.CollectableType);
            Assert.IsEmpty(_messageService.ErrorMessages);
            Assert.IsEmpty(_messageService.WarningMessages);
        }

        private static MetalHolding CreateTestHolding()
        {
            return new MetalHolding
            {
                MetalType = MetalType.Gold,
                Form = "Barren",
                Purity = 999.9m,
                Weight = 10.5m,
                Quantity = 1,
                PurchasePrice = 1000m,
                PurchaseDate = DateTime.Today,
                CollectableType = CollectableType.Bullion
            };
        }

        private sealed class RecordingMessageService : IMessageService
        {
            public List<string> InformationMessages { get; } = new();
            public List<string> WarningMessages { get; } = new();
            public List<string> ErrorMessages { get; } = new();

            public void ShowInformation(string message, string? title = null)
                => InformationMessages.Add(message);

            public void ShowWarning(string message, string title)
                => WarningMessages.Add(message);

            public void ShowError(string message, string title)
                => ErrorMessages.Add(message);

            public bool ShowConfirmation(string message, string title)
                => true;
        }

        private sealed class StubTextProvider : ITextProvider
        {
            public string GetString(string key) => key;
        }

        private void InsertHoldingRowWithNullCollectableType()
        {
            using var connection = new SqliteConnection($"Data Source={_testDbPath}");
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText =
                @"INSERT INTO Holdings
                    (MetalType, Form, Purity, Weight, Quantity, PurchasePrice, PurchaseDate, CollectableType)
                  VALUES
                    (@MetalType, @Form, @Purity, @Weight, @Quantity, @PurchasePrice, @PurchaseDate, NULL);";

            cmd.Parameters.AddWithValue("@MetalType", (int)MetalType.Gold);
            cmd.Parameters.AddWithValue("@Form", "LegacyRow");
            cmd.Parameters.AddWithValue("@Purity", 999.9m);
            cmd.Parameters.AddWithValue("@Weight", 1m);
            cmd.Parameters.AddWithValue("@Quantity", 1);
            cmd.Parameters.AddWithValue("@PurchasePrice", 100m);
            cmd.Parameters.AddWithValue("@PurchaseDate", DateTime.Today.ToString("o"));

            cmd.ExecuteNonQuery();
        }
    }
}
