using Microsoft.VisualStudio.TestTools.UnitTesting;
using PreciousMetalsManager.ViewModels;
using PreciousMetalsManager.Models;
using PreciousMetalsManager.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using System;
using System.IO;

namespace PreciousMetalsManager.Tests
{
    [TestClass]
    public class ViewModelTest
    {
        private string _testDbPath = null!;
        private ViewModel _vm = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _testDbPath = Path.Combine(Path.GetTempPath(), $"test_holdings_{Guid.NewGuid()}.db");
            var storage = new LocalStorageService(_testDbPath);
            _vm = new ViewModel(storage);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _vm = null!;
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
        public void AddHolding_AddsItemToHoldings()
        {
            var holding = new MetalHolding { MetalType = MetalType.Gold, Form = "Barren" };

            _vm.AddHolding(holding);

            Assert.IsTrue(_vm.Holdings.Any(h => h.Form == "Barren" && h.MetalType == MetalType.Gold));
        }

        [TestMethod]
        public void DeleteHolding_RemovesItemFromHoldings()
        {
            var holding = new MetalHolding { MetalType = MetalType.Silver, Form = "Münze" };
            _vm.AddHolding(holding);

            // CHecks if the holding was added
            Assert.IsTrue(_vm.Holdings.Any(h => h.Form == "Münze" && h.MetalType == MetalType.Silver));

            _vm.DeleteHolding(holding);

            // Checks if the holding was removed
            Assert.IsFalse(_vm.Holdings.Any(h => h.Form == "Münze" && h.MetalType == MetalType.Silver));
        }

        [TestMethod]
        public void FormFilter_FiltersHoldings()
        {
            var vm = CreateTestViewModel();
            var holding = new MetalHolding { Form = "Barren" };
            vm.Holdings.Add(holding);

            vm.FormFilter = "Bar";
            Assert.IsTrue(vm.FilteredHoldings.Cast<MetalHolding>().Contains(holding));

            vm.FormFilter = "Münze";
            Assert.IsFalse(vm.FilteredHoldings.Cast<MetalHolding>().Contains(holding));
        }

        [TestMethod]
        public void SelectedMetalTypeFilter_FiltersHoldings()
        {
            var vm = CreateTestViewModel();
            var gold = new MetalHolding { MetalType = MetalType.Gold };
            var silver = new MetalHolding { MetalType = MetalType.Silver };
            vm.Holdings.Add(gold);
            vm.Holdings.Add(silver);

            vm.SelectedMetalTypeFilter = MetalType.Gold;
            var filtered = vm.FilteredHoldings.Cast<MetalHolding>().ToList();

            CollectionAssert.Contains(filtered, gold);
            CollectionAssert.DoesNotContain(filtered, silver);
        }

        [TestMethod]
        public void PropertyChanged_IsRaised_WhenGoldPriceChanges()
        {
            var vm = CreateTestViewModel();
            bool eventRaised = false;
            vm.PropertyChanged += (s, e) => { if (e.PropertyName == nameof(vm.GoldPrice)) eventRaised = true; };

            vm.GoldPrice += 1;

            Assert.IsTrue(eventRaised);
        }

        [TestMethod]
        public void UpdateCalculatedValues_UpdatesCurrentAndTotalValue()
        {
            var vm = CreateTestViewModel();
            var holding = new MetalHolding
            {
                MetalType = MetalType.Gold,
                Weight = 2m,
                Purity = 999.9m,
                Quantity = 3
            };
            vm.Holdings.Add(holding);

            // Current GoldPrice will never be as low as 10m in reallity
            vm.GoldPrice = 10m;
            // UpdateCalculatedValues gets called automatically on property changes

            Assert.AreEqual(2m * (999.9m / 999.9m) * 10m, holding.CurrentValue);
            Assert.AreEqual(holding.CurrentValue * 3, holding.TotalValue);
        }

        [TestMethod]
        public async Task UpdateMarketPricesAsync_UpdatesPricesAndRecalculatesHoldings()
        {
            // Test: API prices are loaded, converted to gram, rounded and holdings recalculated
            var testService = new TestMetalPriceApiService
            {
                ResponseToReturn = new MetalPriceApiResponse
                {
                    GoldEur = 3110m,      // 1g = 100.00
                    SilverEur = 62.2m,    // 1g = 2.00
                    PlatinumEur = 155.5m, // 1g = 5.00
                    PalladiumEur = 62.2m  // 1g = 2.00
                }
            };
            var vm = CreateTestViewModelWithApi(testService);

            var holding = new MetalHolding
            {
                MetalType = MetalType.Gold,
                Weight = 2m,
                Purity = 999.9m,
                Quantity = 1
            };
            vm.Holdings.Add(holding);

            await vm.UpdateMarketPricesAsync();

            Assert.AreEqual(100.00m, vm.GoldPrice);
            Assert.AreEqual(2.00m, vm.SilverPrice);
            Assert.AreEqual(5.00m, vm.PlatinumPrice);
            Assert.AreEqual(2.00m, vm.PalladiumPrice);

            Assert.AreEqual(200.00m, holding.CurrentValue);
            Assert.AreEqual(200.00m, holding.TotalValue);
        }

        [TestMethod]
        public async Task UpdateMarketPricesAsync_ApiError_ShowsErrorMessage()
        {
            // Test: If API fails, error message is shown
            var testService = new TestMetalPriceApiService { ResponseToReturn = null };
            var vm = CreateTestViewModelWithApi(testService);

            await vm.UpdateMarketPricesAsync();

            Assert.IsNotNull(vm.LastErrorMessage);
            Assert.IsNotNull(vm.LastErrorTitle);
        }

        [TestMethod]
        public void AutoRefreshTimer_IsStarted()
        {
            // Test: The auto-refresh timer is started on ViewModel construction
            var testService = new TestMetalPriceApiService();
            var storage = new LocalStorageService(_testDbPath);
            var vm = new TestViewModel(testService, storage);

            var timerField = typeof(ViewModel).GetField("_autoRefreshTimer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var timer = timerField?.GetValue(vm) as DispatcherTimer;

            Assert.IsNotNull(timer);
            Assert.IsTrue(timer.IsEnabled);
        }

        [TestMethod]
        public async Task RefreshPricesCommand_ExecutesUpdateMarketPricesAsync()
        {
            // Test: The RefreshPricesCommand triggers a price update
            var testService = new TestMetalPriceApiService
            {
                ResponseToReturn = new MetalPriceApiResponse
                {
                    GoldEur = 3110m,
                    SilverEur = 62.2m,
                    PlatinumEur = 155.5m,
                    PalladiumEur = 62.2m
                }
            };
            var storage = new LocalStorageService(_testDbPath);
            var vm = new TestViewModel(testService, storage);

            await Task.Run(() => ((ICommand)vm.RefreshPricesCommand).Execute(null));

            Assert.AreEqual(100.00m, vm.GoldPrice);
        }

        [TestMethod]
        public void ToggleLanguage_ResetsFilterToLocalizedAll()
        {
            if (System.Windows.Application.Current == null)
                new PreciousMetalsManager.App();

            var vm = CreateTestViewModel();
            vm.SelectedMetalTypeFilter = MetalType.Gold;

            vm.ToggleLanguage();

            var expectedAll = vm.MetalTypeFilterOptions.First();
            Assert.AreEqual(expectedAll, vm.SelectedMetalTypeFilter);
        }

        [TestMethod]
        public void MetalTypeFilterOptions_FirstEntry_IsAllOption()
        {
            var vm = CreateTestViewModel();

            Assert.IsGreaterThanOrEqualTo(1, vm.MetalTypeFilterOptions.Count);

            var first = vm.MetalTypeFilterOptions[0];
            Assert.IsInstanceOfType(first, typeof(string));
            Assert.AreEqual(first, vm.SelectedMetalTypeFilter); 
        }

        [TestMethod]
        public void MetalTypeFilterOptions_ContainsDistinctMetalTypesOnly()
        {
            var vm = CreateTestViewModel();

            vm.Holdings.Add(new MetalHolding { MetalType = MetalType.Gold, Form = "A" });
            vm.Holdings.Add(new MetalHolding { MetalType = MetalType.Gold, Form = "B" });
            vm.Holdings.Add(new MetalHolding { MetalType = MetalType.Silver, Form = "C" });

            var metalTypes = vm.MetalTypeFilterOptions.OfType<MetalType>().ToList();

            CollectionAssert.AreEquivalent(new List<MetalType> { MetalType.Gold, MetalType.Silver }, metalTypes);

            Assert.AreEqual(metalTypes.Count, metalTypes.Distinct().Count());
        }

        [TestMethod]
        public void SelectedMetalTypeFilter_AllOption_DoesNotFilterByMetalType()
        {
            var vm = CreateTestViewModel();

            var gold = new MetalHolding { MetalType = MetalType.Gold, Form = "Barren" };
            var silver = new MetalHolding { MetalType = MetalType.Silver, Form = "Coin" };
            vm.Holdings.Add(gold);
            vm.Holdings.Add(silver);

            vm.SelectedMetalTypeFilter = vm.MetalTypeFilterOptions.First(); 
            var filtered = vm.FilteredHoldings.Cast<MetalHolding>().ToList();

            CollectionAssert.Contains(filtered, gold);
            CollectionAssert.Contains(filtered, silver);
        }

        [TestMethod]
        // Users curently can't edit the filter options, but this test may be usefull to spot errors in the code
        public void SelectedMetalTypeFilter_UnknownString_DoesNotFilterByMetalType()
        {
            var vm = CreateTestViewModel();

            var gold = new MetalHolding { MetalType = MetalType.Gold, Form = "Barren" };
            var silver = new MetalHolding { MetalType = MetalType.Silver, Form = "Coin" };
            vm.Holdings.Add(gold);
            vm.Holdings.Add(silver);

            // Any string that is not a MetalType must behave like "All" (no MetalType filter)
            vm.SelectedMetalTypeFilter = "SomeUnknownOption";
            var filtered = vm.FilteredHoldings.Cast<MetalHolding>().ToList();

            CollectionAssert.Contains(filtered, gold);
            CollectionAssert.Contains(filtered, silver);
        }

        [TestMethod]
        public void ToggleLanguage_UpdatesAllOptionText_EnToDe()
        {
            if (System.Windows.Application.Current == null)
                new System.Windows.Application { ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown };

            var vm = CreateTestViewModel();

            PreciousMetalsManager.App.SetLanguage("en");
            vm.ToggleLanguage(); // toggles to "de"

            var first = vm.MetalTypeFilterOptions.First();
            Assert.IsInstanceOfType(first, typeof(string));
            Assert.AreEqual(first, vm.SelectedMetalTypeFilter);
        }

        [TestMethod]
        public void ToggleLanguage_UpdatesAllOptionText_DeToEn()
        {
            if (System.Windows.Application.Current == null)
                new System.Windows.Application { ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown };

            var vm = CreateTestViewModel();

            PreciousMetalsManager.App.SetLanguage("de");
            vm.ToggleLanguage(); // toggles to "en"

            var first = vm.MetalTypeFilterOptions.First();
            Assert.IsInstanceOfType(first, typeof(string));
            Assert.AreEqual(first, vm.SelectedMetalTypeFilter);
        }

        [TestMethod]
        public void UpdateMetalTypeFilterOptions_AfterHoldingsChange_AlwaysKeepsAllAtIndex0()
        {
            var vm = CreateTestViewModel();

            vm.Holdings.Add(new MetalHolding { MetalType = MetalType.Gold, Form = "A" });
            Assert.IsInstanceOfType(vm.MetalTypeFilterOptions[0], typeof(string));

            vm.Holdings.Add(new MetalHolding { MetalType = MetalType.Silver, Form = "B" });
            Assert.IsInstanceOfType(vm.MetalTypeFilterOptions[0], typeof(string));

            vm.Holdings.Remove(vm.Holdings.First());
            Assert.IsInstanceOfType(vm.MetalTypeFilterOptions[0], typeof(string));
        }

        [TestMethod]
        public void AddHolding_WithCollectableType_ReflectsInHoldings()
        {
            var vm = CreateTestViewModel();
            var holding = new MetalHolding
            {
                MetalType = MetalType.Gold,
                Form = "Test",
                Purity = 999.9m,
                Weight = 1m,
                Quantity = 1,
                PurchasePrice = 100m,
                PurchaseDate = DateTime.Today,
                CollectableType = CollectableType.SemiNumismatic
            };

            vm.AddHolding(holding);

            Assert.IsTrue(vm.Holdings.Any(h => h.CollectableType == CollectableType.SemiNumismatic));
        }

        [TestMethod]
        public void EditHolding_UpdatesCollectableType()
        {
            var vm = CreateTestViewModel();
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

            vm.AddHolding(holding);

            holding.CollectableType = CollectableType.Numismatic;
            vm.UpdateHolding(holding);

            Assert.IsTrue(vm.Holdings.Any(h => h.Id == holding.Id && h.CollectableType == CollectableType.Numismatic));
        }

        [TestMethod]
        public void TaxFreeOnly_Filter_WorksCorrectly()
        {
            var vm = CreateTestViewModel();

            var now = DateTime.Today;
            var old = new MetalHolding { PurchaseDate = now.AddYears(-2) };
            var young = new MetalHolding { PurchaseDate = now.AddMonths(-6) };

            vm.Holdings.Add(old);
            vm.Holdings.Add(young);

            vm.TaxFreeOnly = true;
            var filtered = vm.FilteredHoldings.Cast<MetalHolding>().ToList();

            Assert.Contains(old, filtered);
            Assert.DoesNotContain(young, filtered);

            vm.TaxFreeOnly = false;
            filtered = vm.FilteredHoldings.Cast<MetalHolding>().ToList();

            Assert.Contains(old, filtered);
            Assert.Contains(young, filtered);
        }

        private ViewModel CreateTestViewModel()
        {
            var storage = new LocalStorageService(_testDbPath);
            return new ViewModel(storage);
        }

        private TestViewModel CreateTestViewModelWithApi(MetalPriceApiService apiService)
        {
            var storage = new LocalStorageService(_testDbPath);
            return new TestViewModel(apiService, storage);
        }

        private class TestMetalPriceApiService : MetalPriceApiService
        {
            public MetalPriceApiResponse? ResponseToReturn { get; set; }
            public override Task<MetalPriceApiResponse?> FetchMetalPricesAsync()
            {
                return Task.FromResult(ResponseToReturn);
            }
        }

        private class TestViewModel : ViewModel
        {
            public string? LastErrorMessage { get; private set; }
            public string? LastErrorTitle { get; private set; }

            public TestViewModel(MetalPriceApiService service, LocalStorageService? storage = null)
                : base(storage)
            {
                // Inject the test service via reflection
                var field = typeof(ViewModel).GetField("_metalPriceApiService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(this, service);
            }

            protected override void ShowErrorMessage(string message, string title)
            {
                LastErrorMessage = message;
                LastErrorTitle = title;
            }
        }
    }
}
