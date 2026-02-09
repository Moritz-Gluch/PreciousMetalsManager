using Microsoft.VisualStudio.TestTools.UnitTesting;
using PreciousMetalsManager.Services;
using PreciousMetalsManager.Models;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace PreciousMetalsManager.Tests
{
    [TestClass]
    public class LocalStorageServiceTest
    {
        private string _testDbPath = null!;
        private LocalStorageService _service = null!; 

        [TestInitialize]
        public void TestInitialize()
        {
            // Creates a unique temporary database for each test
            _testDbPath = Path.Combine(Path.GetTempPath(), $"test_holdings_{Guid.NewGuid()}.db");
            _service = new LocalStorageService(_testDbPath);
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
        }

        [TestMethod]
        public void DeleteHolding_ShouldRemoveHolding()
        {
            var holding = CreateTestHolding();
            _service.AddHolding(holding);

            // Ensure the holding was actually added
            var holdingsBeforeDelete = _service.LoadHoldings();
            Assert.IsTrue(holdingsBeforeDelete.Any(h => h.Id == holding.Id));

            _service.DeleteHolding(holding.Id);
            var holdingsAfterDelete = _service.LoadHoldings();

            Assert.IsFalse(holdingsAfterDelete.Any(h => h.Id == holding.Id));
        }

        [TestMethod]
        public void LoadHoldings_ShouldReturnList()
        {
            var holdings = _service.LoadHoldings();

            Assert.IsNotNull(holdings);
            Assert.IsInstanceOfType(holdings, typeof(List<MetalHolding>));
        }

        /// <summary>
        /// Creates a test object with all required fields.
        /// </summary>
        private MetalHolding CreateTestHolding()
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
    }
}
