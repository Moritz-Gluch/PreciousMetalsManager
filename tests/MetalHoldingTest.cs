using Microsoft.VisualStudio.TestTools.UnitTesting;
using PreciousMetalsManager.Models;
using System;
using System.ComponentModel;

namespace PreciousMetalsManager.Tests
{
    [TestClass]
    public class MetalHoldingTest
    {
        [TestMethod]
        public void Properties_SetAndGetValues_Correctly()
        {
            var holding = new MetalHolding();

            holding.Id = 1;
            holding.MetalType = MetalType.Gold;
            holding.Form = "Barren";
            holding.Purity = 999.9m;
            holding.Weight = 10.5m;
            holding.Quantity = 2;
            holding.PurchasePrice = 1000m;
            var date = new DateTime(2026, 1, 28);
            holding.PurchaseDate = date;
            holding.CurrentValue = 1200m;
            holding.TotalValue = 2400m;

            Assert.AreEqual(1, holding.Id);
            Assert.AreEqual(MetalType.Gold, holding.MetalType);
            Assert.AreEqual("Barren", holding.Form);
            Assert.AreEqual(999.9m, holding.Purity);
            Assert.AreEqual(10.5m, holding.Weight);
            Assert.AreEqual(2, holding.Quantity);
            Assert.AreEqual(1000m, holding.PurchasePrice);
            Assert.AreEqual(date, holding.PurchaseDate);
            Assert.AreEqual(1200m, holding.CurrentValue);
            Assert.AreEqual(2400m, holding.TotalValue);
        }

        [TestMethod]
        public void PropertyChanged_IsRaised_WhenPropertyChanges()
        {
            var holding = new MetalHolding();
            string? changedProperty = null;
            holding.PropertyChanged += (s, e) => changedProperty = e.PropertyName;

            holding.Weight = 42m;

            Assert.AreEqual(nameof(MetalHolding.Weight), changedProperty);
        }

        [TestMethod]
        public void SettingSameValue_DoesNotRaisePropertyChanged()
        {
            var holding = new MetalHolding();
            holding.Form = "Barren";
            bool eventRaised = false;
            holding.PropertyChanged += (s, e) => eventRaised = true;

            holding.Form = "Barren"; // Set to same value

            Assert.IsFalse(eventRaised);
        }

        [TestMethod]
        public void DefaultValues_AreCorrect()
        {
            var holding = new MetalHolding();

            Assert.AreEqual(0, holding.Id);
            Assert.AreEqual(string.Empty, holding.Form);
            Assert.AreEqual(0m, holding.Purity);
            Assert.AreEqual(0m, holding.Weight);
            Assert.AreEqual(0, holding.Quantity);
            Assert.AreEqual(0m, holding.PurchasePrice);
            Assert.AreEqual(default(DateTime), holding.PurchaseDate);
            Assert.AreEqual(0m, holding.CurrentValue);
            Assert.AreEqual(0m, holding.TotalValue);
        }
    }
}
