using Microsoft.VisualStudio.TestTools.UnitTesting;
using PreciousMetalsManager.Models;
using System;
using System.Collections.Generic;

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
            holding.CollectableType = CollectableType.SemiNumismatic;

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
            Assert.AreEqual(CollectableType.SemiNumismatic, holding.CollectableType);
        }

        [TestMethod]
        public void PropertyChanged_IsRaised_WhenPropertyChanges()
        {
            var holding = new MetalHolding();
            string? changedProperty = null;
            holding.PropertyChanged += (_, e) => changedProperty = e.PropertyName;

            holding.Weight = 42m;

            Assert.AreEqual(nameof(MetalHolding.Weight), changedProperty);
        }

        [TestMethod]
        public void SettingSameValue_DoesNotRaisePropertyChanged()
        {
            var holding = new MetalHolding();
            holding.Form = "Barren";
            var eventRaised = false;
            holding.PropertyChanged += (_, _) => eventRaised = true;

            holding.Form = "Barren";

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
            Assert.AreEqual(default(CollectableType), holding.CollectableType);
        }

        [TestMethod]
        public void IsTaxFree_IsTrue_ForHoldingOlderThanOneYear()
        {
            var holding = new MetalHolding
            {
                PurchaseDate = DateTime.Today.AddYears(-1).AddDays(-1)
            };

            Assert.IsTrue(holding.IsTaxFree);
        }

        [TestMethod]
        public void IsTaxFree_IsTrue_OnExactOneYearBoundary()
        {
            var holding = new MetalHolding
            {
                PurchaseDate = DateTime.Today.AddYears(-1)
            };

            Assert.IsTrue(holding.IsTaxFree);
        }

        [TestMethod]
        public void IsTaxFree_IsFalse_ForHoldingNewerThanOneYear()
        {
            var holding = new MetalHolding
            {
                PurchaseDate = DateTime.Today.AddMonths(-6)
            };

            Assert.IsFalse(holding.IsTaxFree);
        }

        [TestMethod]
        public void IsTaxFree_IsFalse_WhenPurchaseDateIsDefault()
        {
            var holding = new MetalHolding();

            Assert.IsFalse(holding.IsTaxFree);
        }

        [TestMethod]
        public void TaxFreeDaysLeft_ReturnsMaxValue_WhenPurchaseDateIsDefault()
        {
            var holding = new MetalHolding();

            Assert.AreEqual(int.MaxValue, holding.TaxFreeDaysLeft);
        }

        [TestMethod]
        public void TaxFreeDaysLeft_ReturnsZero_WhenHoldingIsTaxFree()
        {
            var holding = new MetalHolding
            {
                PurchaseDate = DateTime.Today.AddYears(-2)
            };

            Assert.AreEqual(0, holding.TaxFreeDaysLeft);
        }

        [TestMethod]
        public void TaxFreeDaysLeft_ReturnsOne_OnDayBeforeTaxFree()
        {
            var holding = new MetalHolding
            {
                PurchaseDate = DateTime.Today.AddYears(-1).AddDays(1)
            };

            Assert.AreEqual(1, holding.TaxFreeDaysLeft);
        }

        [TestMethod]
        public void TaxFreeDaysLeft_ReturnsRemainingDays_WhenHoldingIsNotTaxFree()
        {
            var purchaseDate = DateTime.Today.AddMonths(-6);
            var expected = (purchaseDate.AddYears(1) - DateTime.Today).Days;

            var holding = new MetalHolding
            {
                PurchaseDate = purchaseDate
            };

            Assert.AreEqual(expected, holding.TaxFreeDaysLeft);
        }

        [TestMethod]
        public void TaxFreeSortValue_IsZero_ForTaxFree_OtherwiseDaysLeft()
        {
            var now = DateTime.Today;
            var oneYearAgo = now.AddYears(-1).AddDays(-1);
            var almostOneYearAgo = now.AddYears(-1).AddDays(1);

            var holdingOld = new MetalHolding { PurchaseDate = oneYearAgo };
            var holdingNew = new MetalHolding { PurchaseDate = almostOneYearAgo };

            Assert.AreEqual(0, holdingOld.TaxFreeSortValue);
            Assert.AreEqual((almostOneYearAgo.AddYears(1) - now).Days, holdingNew.TaxFreeSortValue);
        }

        [TestMethod]
        public void NotifyTaxFreeStatusChanged_RaisesAllRelatedProperties()
        {
            var holding = new MetalHolding();
            var changedProperties = new List<string>();

            holding.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName is not null)
                    changedProperties.Add(e.PropertyName);
            };

            holding.NotifyTaxFreeStatusChanged();

            CollectionAssert.AreEquivalent(
                new[]
                {
                    nameof(MetalHolding.IsTaxFree),
                    nameof(MetalHolding.TaxFreeDaysLeft),
                    nameof(MetalHolding.TaxFreeSortValue)
                },
                changedProperties);
        }
    }
}