using Microsoft.VisualStudio.TestTools.UnitTesting;
using PreciousMetalsManager.ViewModels;
using System;

namespace PreciousMetalsManager.Tests
{
    [TestClass]
    public class EditPricesDialogViewModelTest
    {
        [TestMethod]
        public void Constructor_FormatsInitialValues_AndStoresPriceUnit()
        {
            var vm = new EditPricesDialogViewModel(
                100m,
                2.5m,
                3.75m,
                4m,
                1.2m,
                "€/g");

            Assert.AreEqual("100.00", vm.GoldPriceText);
            Assert.AreEqual("2.50", vm.SilverPriceText);
            Assert.AreEqual("3.75", vm.PlatinumPriceText);
            Assert.AreEqual("4.00", vm.PalladiumPriceText);
            Assert.AreEqual("1.20", vm.BroncePriceText);
            Assert.AreEqual("€/g", vm.PriceUnit);
        }

        [TestMethod]
        public void NormalizeGoldPrice_FormatsCommaInput_AsInvariant()
        {
            var vm = new EditPricesDialogViewModel(0, 0, 0, 0, 0, "€/g")
            {
                GoldPriceText = "12,5"
            };

            vm.NormalizeGoldPrice();

            Assert.AreEqual("12.50", vm.GoldPriceText);
        }

        [TestMethod]
        public void NormalizeGoldPrice_ShouldResetToDefault_WhenInvalid()
        {
            var vm = new EditPricesDialogViewModel(0m, 0m, 0m, 0m, 0m, "€/g")
            {
                GoldPriceText = "abc"
            };

            vm.NormalizeGoldPrice();

            Assert.AreEqual("0.00", vm.GoldPriceText);
        }

        [TestMethod]
        public void NormalizeSilverPrice_ShouldAcceptCommaInput_AndRound()
        {
            var vm = new EditPricesDialogViewModel(0m, 0m, 0m, 0m, 0m, "€/g")
            {
                SilverPriceText = "2,345"
            };

            vm.NormalizeSilverPrice();

            Assert.AreEqual("2.35", vm.SilverPriceText);
        }

        [TestMethod]
        public void NormalizeSilverPrice_InvalidInput_ResetsToDefault()
        {
            var vm = new EditPricesDialogViewModel(0, 0, 0, 0, 0, "€/g")
            {
                SilverPriceText = "abc"
            };

            vm.NormalizeSilverPrice();

            Assert.AreEqual("0.00", vm.SilverPriceText);
        }

        [TestMethod]
        public void NormalizePlatinumPrice_ShouldRoundToTwoDecimals()
        {
            var vm = new EditPricesDialogViewModel(0m, 0m, 0m, 0m, 0m, "€/g")
            {
                PlatinumPriceText = "123.456"
            };

            vm.NormalizePlatinumPrice();

            Assert.AreEqual("123.46", vm.PlatinumPriceText);
        }

        [TestMethod]
        public void NormalizePlatinumPrice_NegativeInput_ResetsToDefault()
        {
            var vm = new EditPricesDialogViewModel(0, 0, 0, 0, 0, "€/g")
            {
                PlatinumPriceText = "-1"
            };

            vm.NormalizePlatinumPrice();

            Assert.AreEqual("0.00", vm.PlatinumPriceText);
        }

        [TestMethod]
        public void NormalizePalladiumPrice_ShouldAcceptIntegerInput()
        {
            var vm = new EditPricesDialogViewModel(0m, 0m, 0m, 0m, 0m, "€/g")
            {
                PalladiumPriceText = "7"
            };

            vm.NormalizePalladiumPrice();

            Assert.AreEqual("7.00", vm.PalladiumPriceText);
        }

        [TestMethod]
        public void NormalizePalladiumPrice_ShouldResetToDefault_WhenInvalid()
        {
            var vm = new EditPricesDialogViewModel(0m, 0m, 0m, 0m, 0m, "€/g")
            {
                PalladiumPriceText = "invalid"
            };

            vm.NormalizePalladiumPrice();

            Assert.AreEqual("0.00", vm.PalladiumPriceText);
        }

        [TestMethod]
        public void NormalizeBroncePrice_ShouldResetToDefault_WhenNegative()
        {
            var vm = new EditPricesDialogViewModel(0m, 0m, 0m, 0m, 0m, "€/g")
            {
                BroncePriceText = "-1"
            };

            vm.NormalizeBroncePrice();

            Assert.AreEqual("0.00", vm.BroncePriceText);
        }

        [TestMethod]
        public void NormalizeBroncePrice_ShouldAcceptValidInput_AndRound()
        {
            var vm = new EditPricesDialogViewModel(0m, 0m, 0m, 0m, 0m, "€/g")
            {
                BroncePriceText = "1,234"
            };

            vm.NormalizeBroncePrice();

            Assert.AreEqual("1.23", vm.BroncePriceText);
        }

        [TestMethod]
        public void NormalizeBroncePrice_RoundsToTwoDecimals()
        {
            var vm = new EditPricesDialogViewModel(0, 0, 0, 0, 0, "€/g")
            {
                BroncePriceText = "1,235"
            };

            vm.NormalizeBroncePrice();

            Assert.AreEqual("1.24", vm.BroncePriceText);
        }

        [TestMethod]
        public void SaveCommand_WithValidInput_SetsPricesAndRequestsClose()
        {
            var vm = new EditPricesDialogViewModel(0, 0, 0, 0, 0, "€/g")
            {
                GoldPriceText = "100,50",
                SilverPriceText = "20.25",
                PlatinumPriceText = "30",
                PalladiumPriceText = "40,75",
                BroncePriceText = "5"
            };

            bool? accepted = null;
            vm.RequestCloseRequested += (_, value) => accepted = value;

            vm.SaveCommand.Execute(null);

            Assert.AreEqual(true, accepted);
            Assert.AreEqual(100.50m, vm.GoldPrice);
            Assert.AreEqual(20.25m, vm.SilverPrice);
            Assert.AreEqual(30m, vm.PlatinumPrice);
            Assert.AreEqual(40.75m, vm.PalladiumPrice);
            Assert.AreEqual(5m, vm.BroncePrice);
        }

        [TestMethod]
        public void SaveCommand_WithInvalidInput_DoesNotRequestClose_AndDoesNotApplyValues()
        {
            var vm = new EditPricesDialogViewModel(0, 0, 0, 0, 0, "€/g")
            {
                GoldPriceText = "-1",
                SilverPriceText = "2.00",
                PlatinumPriceText = "3.00",
                PalladiumPriceText = "4.00",
                BroncePriceText = "5.00"
            };

            bool? accepted = null;
            vm.RequestCloseRequested += (_, value) => accepted = value;

            vm.SaveCommand.Execute(null);

            Assert.IsNull(accepted);
            Assert.AreEqual(0m, vm.GoldPrice);
            Assert.AreEqual(0m, vm.SilverPrice);
            Assert.AreEqual(0m, vm.PlatinumPrice);
            Assert.AreEqual(0m, vm.PalladiumPrice);
            Assert.AreEqual(0m, vm.BroncePrice);
        }

        [TestMethod]
        public void CancelCommand_RequestsCloseFalse()
        {
            var vm = new EditPricesDialogViewModel(1, 2, 3, 4, 5, "€/g");

            bool? accepted = null;
            vm.RequestCloseRequested += (_, value) => accepted = value;

            vm.CancelCommand.Execute(null);

            Assert.AreEqual(false, accepted);
        }

        [TestMethod]
        public void GoldPriceText_PropertyChanged_IsRaised_WhenValueChanges()
        {
            var vm = new EditPricesDialogViewModel(0m, 0m, 0m, 0m, 0m, "€/g");
            string? changedProperty = null;

            vm.PropertyChanged += (_, e) => changedProperty = e.PropertyName;

            vm.GoldPriceText = "12.34";

            Assert.AreEqual(nameof(EditPricesDialogViewModel.GoldPriceText), changedProperty);
        }

        [TestMethod]
        public void GoldPriceText_PropertyChanged_IsNotRaised_WhenSameValueIsAssigned()
        {
            var vm = new EditPricesDialogViewModel(0m, 0m, 0m, 0m, 0m, "€/g");
            vm.GoldPriceText = "12.34";

            var eventRaised = false;
            vm.PropertyChanged += (_, _) => eventRaised = true;

            vm.GoldPriceText = "12.34";

            Assert.IsFalse(eventRaised);
        }
    }
}