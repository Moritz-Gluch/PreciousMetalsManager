using Microsoft.VisualStudio.TestTools.UnitTesting;
using PreciousMetalsManager.ViewModels;
using PreciousMetalsManager.Models;
using System.Linq;
using System.Collections.Specialized;
using System.ComponentModel;

namespace PreciousMetalsManager.Tests
{
    [TestClass]
    public class ViewModelTest
    {
        [TestMethod]
        public void AddHolding_AddsItemToHoldings()
        {
            var vm = new ViewModel();
            var holding = new MetalHolding { MetalType = MetalType.Gold, Form = "Barren" };

            vm.AddHolding(holding);

            Assert.IsTrue(vm.Holdings.Any(h => h.Form == "Barren" && h.MetalType == MetalType.Gold));
        }

        [TestMethod]
        public void DeleteHolding_RemovesItemFromHoldings()
        {
            var vm = new ViewModel();
            var holding = new MetalHolding { MetalType = MetalType.Silver, Form = "Münze" };
            vm.AddHolding(holding);

            // CHecks if the holding was added
            Assert.IsTrue(vm.Holdings.Any(h => h.Form == "Münze" && h.MetalType == MetalType.Silver));

            vm.DeleteHolding(holding);

            // Checks if the holding was removed
            Assert.IsFalse(vm.Holdings.Any(h => h.Form == "Münze" && h.MetalType == MetalType.Silver));
        }

        [TestMethod]
        public void FormFilter_FiltersHoldings()
        {
            var vm = new ViewModel();
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
            var vm = new ViewModel();
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
            var vm = new ViewModel();
            bool eventRaised = false;
            vm.PropertyChanged += (s, e) => { if (e.PropertyName == nameof(vm.GoldPrice)) eventRaised = true; };

            vm.GoldPrice += 1;

            Assert.IsTrue(eventRaised);
        }

        [TestMethod]
        public void UpdateCalculatedValues_UpdatesCurrentAndTotalValue()
        {
            var vm = new ViewModel();
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
    }
}
