using Microsoft.VisualStudio.TestTools.UnitTesting;
using PreciousMetalsManager.Models;
using PreciousMetalsManager.ViewModels;
using System;

namespace PreciousMetalsManager.Tests
{
    [TestClass]
    public class HoldingDialogViewModelTest
    {
        [TestMethod]
        public void Constructor_SetsExpectedDefaults()
        {
            var vm = new HoldingDialogViewModel();

            Assert.IsFalse(vm.IsEditMode);
            Assert.AreEqual(MetalType.Gold, vm.SelectedMetalType);
            Assert.AreEqual(CollectableType.Bullion, vm.SelectedCollectableType);
            Assert.AreEqual(string.Empty, vm.FormText);
            Assert.AreEqual("999.9", vm.PurityText);
            Assert.AreEqual("1.00", vm.WeightText);
            Assert.AreEqual("1", vm.QuantityText);
            Assert.AreEqual("0.00", vm.PurchasePriceText);
            Assert.AreEqual(DateTime.Today, vm.PurchaseDate);
            Assert.IsNull(vm.CreatedHolding);
            Assert.IsFalse(vm.AddAnotherRequested);
            Assert.IsTrue(vm.AddAnotherCommand.CanExecute(null));
        }

        [TestMethod]
        public void EditConstructor_CopiesHoldingValues_AndDisablesAddAnother()
        {
            var date = new DateTime(2025, 3, 4);
            var holding = new MetalHolding
            {
                MetalType = MetalType.Palladium,
                Form = "Coin",
                Purity = 925.0m,
                Weight = 31.10m,
                Quantity = 3,
                PurchasePrice = 1500.50m,
                PurchaseDate = date,
                CollectableType = CollectableType.Numismatic
            };

            var vm = new HoldingDialogViewModel(holding);

            Assert.IsTrue(vm.IsEditMode);
            Assert.AreEqual(MetalType.Palladium, vm.SelectedMetalType);
            Assert.AreEqual("Coin", vm.FormText);
            Assert.AreEqual("Coin", vm.OriginalFormText);
            Assert.AreEqual("925.0", vm.PurityText);
            Assert.AreEqual("31.10", vm.WeightText);
            Assert.AreEqual("3", vm.QuantityText);
            Assert.AreEqual("1500.50", vm.PurchasePriceText);
            Assert.AreEqual(date, vm.PurchaseDate);
            Assert.AreEqual(CollectableType.Numismatic, vm.SelectedCollectableType);
            Assert.IsFalse(vm.AddAnotherCommand.CanExecute(null));
        }

        [TestMethod]
        public void RestoreFormTextIfNeeded_InEditMode_RestoresOriginalValue()
        {
            var holding = new MetalHolding { Form = "Original" };
            var vm = new HoldingDialogViewModel(holding)
            {
                FormText = ""
            };

            vm.RestoreFormTextIfNeeded();

            Assert.AreEqual("Original", vm.FormText);
        }

        [TestMethod]
        public void RestoreFormTextIfNeeded_InCreateMode_DoesNothing()
        {
            var vm = new HoldingDialogViewModel
            {
                FormText = ""
            };

            vm.RestoreFormTextIfNeeded();

            Assert.AreEqual("", vm.FormText);
        }

        [TestMethod]
        public void NormalizePurityText_InvalidInput_ResetsToDefault()
        {
            var vm = new HoldingDialogViewModel
            {
                PurityText = "abc"
            };

            vm.NormalizePurityText();

            Assert.AreEqual("999.9", vm.PurityText);
        }

        [TestMethod]
        public void NormalizePurityText_ClampsAndRoundsValue()
        {
            var vm = new HoldingDialogViewModel
            {
                PurityText = "1000,01"
            };

            vm.NormalizePurityText();

            Assert.AreEqual("999.9", vm.PurityText);

            vm.PurityText = "0";
            vm.NormalizePurityText();

            Assert.AreEqual("0.1", vm.PurityText);

            vm.PurityText = "925,05";
            vm.NormalizePurityText();

            Assert.AreEqual("925.1", vm.PurityText);
        }

        [TestMethod]
        public void NormalizeWeightText_InvalidOrNonPositive_ResetsToDefault()
        {
            var vm = new HoldingDialogViewModel
            {
                WeightText = "0"
            };

            vm.NormalizeWeightText();

            Assert.AreEqual("1.00", vm.WeightText);

            vm.WeightText = "abc";
            vm.NormalizeWeightText();

            Assert.AreEqual("1.00", vm.WeightText);
        }

        [TestMethod]
        public void NormalizeWeightText_RoundsValue()
        {
            var vm = new HoldingDialogViewModel
            {
                WeightText = "1,235"
            };

            vm.NormalizeWeightText();

            Assert.AreEqual("1.24", vm.WeightText);
        }

        [TestMethod]
        public void NormalizeQuantityText_InvalidOrTooSmall_ResetsToDefault()
        {
            var vm = new HoldingDialogViewModel
            {
                QuantityText = "0"
            };

            vm.NormalizeQuantityText();

            Assert.AreEqual("1", vm.QuantityText);

            vm.QuantityText = "abc";
            vm.NormalizeQuantityText();

            Assert.AreEqual("1", vm.QuantityText);
        }

        [TestMethod]
        public void NormalizePurchasePriceText_InvalidOrNegative_ResetsToDefault()
        {
            var vm = new HoldingDialogViewModel
            {
                PurchasePriceText = "-1"
            };

            vm.NormalizePurchasePriceText();

            Assert.AreEqual("0.00", vm.PurchasePriceText);

            vm.PurchasePriceText = "abc";
            vm.NormalizePurchasePriceText();

            Assert.AreEqual("0.00", vm.PurchasePriceText);
        }

        [TestMethod]
        public void NormalizePurchasePriceText_FormatsInvariant()
        {
            var vm = new HoldingDialogViewModel
            {
                PurchasePriceText = "12,5"
            };

            vm.NormalizePurchasePriceText();

            Assert.AreEqual("12.50", vm.PurchasePriceText);
        }

        [TestMethod]
        public void EnsurePurchaseDate_WhenNull_SetsToday()
        {
            var vm = new HoldingDialogViewModel
            {
                PurchaseDate = null
            };

            vm.EnsurePurchaseDate();

            Assert.AreEqual(DateTime.Today, vm.PurchaseDate);
        }

        [TestMethod]
        public void IncreaseQuantityCommand_IncrementsQuantity()
        {
            var vm = new HoldingDialogViewModel
            {
                QuantityText = "2"
            };

            vm.IncreaseQuantityCommand.Execute(null);

            Assert.AreEqual("3", vm.QuantityText);
        }

        [TestMethod]
        public void DecreaseQuantityCommand_DecrementsQuantity_ButNotBelowMinimum()
        {
            var vm = new HoldingDialogViewModel
            {
                QuantityText = "2"
            };

            vm.DecreaseQuantityCommand.Execute(null);
            Assert.AreEqual("1", vm.QuantityText);

            vm.DecreaseQuantityCommand.Execute(null);
            Assert.AreEqual("1", vm.QuantityText);
        }

        [TestMethod]
        public void SaveCommand_WithValidInput_CreatesHoldingAndRequestsClose()
        {
            var vm = new HoldingDialogViewModel
            {
                SelectedMetalType = MetalType.Silver,
                SelectedCollectableType = CollectableType.SemiNumismatic,
                FormText = "  Maple Leaf  ",
                PurityText = "999,9",
                WeightText = "31,10",
                QuantityText = "2",
                PurchasePriceText = "1234,56",
                PurchaseDate = new DateTime(2024, 12, 24)
            };

            bool? accepted = null;
            vm.RequestCloseRequested += (_, value) => accepted = value;

            vm.SaveCommand.Execute(null);

            Assert.IsFalse(accepted);
            Assert.IsNotNull(vm.CreatedHolding);
            Assert.IsFalse(vm.AddAnotherRequested);

            Assert.AreEqual(MetalType.Silver, vm.CreatedHolding.MetalType);
            Assert.AreEqual(CollectableType.SemiNumismatic, vm.CreatedHolding.CollectableType);
            Assert.AreEqual("Maple Leaf", vm.CreatedHolding.Form);
            Assert.AreEqual(999.9m, vm.CreatedHolding.Purity);
            Assert.AreEqual(31.10m, vm.CreatedHolding.Weight);
            Assert.AreEqual(2, vm.CreatedHolding.Quantity);
            Assert.AreEqual(1234.56m, vm.CreatedHolding.PurchasePrice);
            Assert.AreEqual(new DateTime(2024, 12, 24), vm.CreatedHolding.PurchaseDate);
        }

        [TestMethod]
        public void SaveCommand_WithEmptyForm_RaisesFocusRequest_AndDoesNotClose()
        {
            var vm = new HoldingDialogViewModel
            {
                FormText = "   ",
                PurityText = "999.9",
                WeightText = "1.00",
                QuantityText = "1",
                PurchasePriceText = "0.00",
                PurchaseDate = DateTime.Today
            };

            var focusRequested = false;
            bool? accepted = null;

            vm.RequestFormTextFocus += (_, _) => focusRequested = true;
            vm.RequestCloseRequested += (_, value) => accepted = value;

            vm.SaveCommand.Execute(null);

            Assert.IsTrue(focusRequested);
            Assert.IsNull(accepted);
            Assert.IsNull(vm.CreatedHolding);
        }

        [TestMethod]
        public void AddAnotherCommand_WithValidInput_CreatesHoldingAndSetsFlag()
        {
            var vm = new HoldingDialogViewModel
            {
                FormText = "Krugerrand",
                PurityText = "916.7",
                WeightText = "33.93",
                QuantityText = "1",
                PurchasePriceText = "2500.00",
                PurchaseDate = new DateTime(2025, 1, 1)
            };

            bool? accepted = null;
            vm.RequestCloseRequested += (_, value) => accepted = value;

            vm.AddAnotherCommand.Execute(null);

            Assert.IsFalse(accepted);
            Assert.IsTrue(vm.AddAnotherRequested);
            Assert.IsNotNull(vm.CreatedHolding);
            Assert.AreEqual("Krugerrand", vm.CreatedHolding.Form);
        }

        [TestMethod]
        public void CancelCommand_ClearsCreatedHolding_AndRequestsCloseFalse()
        {
            var vm = new HoldingDialogViewModel
            {
                FormText = "Test",
                PurityText = "999.9",
                WeightText = "1.00",
                QuantityText = "1",
                PurchasePriceText = "1.00",
                PurchaseDate = DateTime.Today
            };

            vm.SaveCommand.Execute(null);
            Assert.IsNotNull(vm.CreatedHolding);

            bool? accepted = null;
            vm.RequestCloseRequested += (_, value) => accepted = value;

            vm.CancelCommand.Execute(null);

            Assert.IsFalse(accepted);
            Assert.IsNull(vm.CreatedHolding);
            Assert.IsFalse(vm.AddAnotherRequested);
        }

        [TestMethod]
        public void FormText_PropertyChanged_IsRaised_WhenValueChanges()
        {
            var vm = new HoldingDialogViewModel();
            string? changedProperty = null;

            vm.PropertyChanged += (_, e) => changedProperty = e.PropertyName;

            vm.FormText = "Krugerrand";

            Assert.AreEqual(nameof(HoldingDialogViewModel.FormText), changedProperty);
        }

        [TestMethod]
        public void FormText_PropertyChanged_IsNotRaised_WhenSameValueIsAssigned()
        {
            var vm = new HoldingDialogViewModel
            {
                FormText = "Krugerrand"
            };

            var eventRaised = false;
            vm.PropertyChanged += (_, _) => eventRaised = true;

            vm.FormText = "Krugerrand";

            Assert.IsFalse(eventRaised);
        }

        [TestMethod]
        public void AddAnotherCommand_WithEmptyForm_RaisesFocusRequest_AndDoesNotClose()
        {
            var vm = new HoldingDialogViewModel
            {
                FormText = "   ",
                PurityText = "999.9",
                WeightText = "1.00",
                QuantityText = "1",
                PurchasePriceText = "0.00",
                PurchaseDate = DateTime.Today
            };

            var focusRequested = false;
            bool? accepted = null;

            vm.RequestFormTextFocus += (_, _) => focusRequested = true;
            vm.RequestCloseRequested += (_, value) => accepted = value;

            vm.AddAnotherCommand.Execute(null);

            Assert.IsTrue(focusRequested);
            Assert.IsNull(accepted);
            Assert.IsNull(vm.CreatedHolding);
            Assert.IsTrue(vm.AddAnotherRequested);
        }
    }
}