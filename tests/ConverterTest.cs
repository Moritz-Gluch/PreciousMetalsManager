using Microsoft.VisualStudio.TestTools.UnitTesting;
using PreciousMetalsManager.Converters;
using PreciousMetalsManager.Models;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PreciousMetalsManager.Tests
{
    [STATestClass]
    public sealed class ConverterTest
    {
        private const string InvalidEnumName = "InvalidValue";
        private const string NonBooleanText = "true";
        private const string ArbitraryDisplayText = "SomeRandomString";
        private const string LowercaseSemiNumismatic = "seminumismatic";

        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            EnsureWpfApp();
        }

        [TestMethod]
        public void BoolToVisibilityConverter_Convert_True_ReturnsVisible()
        {
            var converter = new BoolToVisibilityConverter();

            var result = converter.Convert(true, typeof(Visibility), null!, CultureInfo.InvariantCulture);

            Assert.AreEqual(Visibility.Visible, result);
        }

        [TestMethod]
        public void BoolToVisibilityConverter_Convert_False_ReturnsCollapsed()
        {
            var converter = new BoolToVisibilityConverter();

            var result = converter.Convert(false, typeof(Visibility), null!, CultureInfo.InvariantCulture);

            Assert.AreEqual(Visibility.Collapsed, result);
        }

        [TestMethod]
        public void BoolToVisibilityConverter_Convert_WithInvert_True_ReturnsCollapsed()
        {
            var converter = new BoolToVisibilityConverter
            {
                Invert = true
            };

            var result = converter.Convert(true, typeof(Visibility), null!, CultureInfo.InvariantCulture);

            Assert.AreEqual(Visibility.Collapsed, result);
        }

        [TestMethod]
        public void BoolToVisibilityConverter_Convert_WithInvert_False_ReturnsVisible()
        {
            var converter = new BoolToVisibilityConverter
            {
                Invert = true
            };

            var result = converter.Convert(false, typeof(Visibility), null!, CultureInfo.InvariantCulture);

            Assert.AreEqual(Visibility.Visible, result);
        }

        [TestMethod]
        public void BoolToVisibilityConverter_ConvertBack_Visible_ReturnsTrue()
        {
            var converter = new BoolToVisibilityConverter();

            var result = converter.ConvertBack(Visibility.Visible, typeof(bool), null!, CultureInfo.InvariantCulture);

            Assert.IsTrue((bool)result);
        }

        [TestMethod]
        public void BoolToVisibilityConverter_ConvertBack_Collapsed_ReturnsFalse()
        {
            var converter = new BoolToVisibilityConverter();

            var result = converter.ConvertBack(Visibility.Collapsed, typeof(bool), null!, CultureInfo.InvariantCulture);

            Assert.IsFalse((bool)result);
        }

        [TestMethod]
        public void BoolToVisibilityConverter_ConvertBack_InvalidValue_ReturnsFalse()
        {
            var converter = new BoolToVisibilityConverter();

            var result = converter.ConvertBack(NonBooleanText, typeof(bool), null!, CultureInfo.InvariantCulture);

            Assert.IsFalse((bool)result);
        }

        [TestMethod]
        public void EnumToBooleanConverter_Convert_MatchingValue_ReturnsTrue()
        {
            var converter = new EnumToBooleanConverter();

            var result = converter.Convert(
                CollectableType.Bullion,
                typeof(bool),
                nameof(CollectableType.Bullion),
                CultureInfo.InvariantCulture);

            Assert.IsTrue((bool)result);
        }

        [TestMethod]
        public void EnumToBooleanConverter_Convert_NonMatchingValue_ReturnsFalse()
        {
            var converter = new EnumToBooleanConverter();

            var result = converter.Convert(
                CollectableType.Bullion,
                typeof(bool),
                nameof(CollectableType.Numismatic),
                CultureInfo.InvariantCulture);

            Assert.IsFalse((bool)result);
        }

        [TestMethod]
        public void EnumToBooleanConverter_Convert_IsCaseInsensitive()
        {
            var converter = new EnumToBooleanConverter();

            var result = converter.Convert(
                CollectableType.SemiNumismatic,
                typeof(bool),
                LowercaseSemiNumismatic,
                CultureInfo.InvariantCulture);

            Assert.IsTrue((bool)result);
        }

        [TestMethod]
        public void EnumToBooleanConverter_Convert_WithNullValue_ReturnsFalse()
        {
            var converter = new EnumToBooleanConverter();

            var result = converter.Convert(
                null!,
                typeof(bool),
                nameof(CollectableType.Bullion),
                CultureInfo.InvariantCulture);

            Assert.IsFalse((bool)result);
        }

        [TestMethod]
        public void EnumToBooleanConverter_Convert_WithNullParameter_ReturnsFalse()
        {
            var converter = new EnumToBooleanConverter();

            var result = converter.Convert(
                CollectableType.Bullion,
                typeof(bool),
                null!,
                CultureInfo.InvariantCulture);

            Assert.IsFalse((bool)result);
        }

        [TestMethod]
        public void EnumToBooleanConverter_ConvertBack_TrueAndValidParameter_ReturnsEnumValue()
        {
            var converter = new EnumToBooleanConverter();

            var result = converter.ConvertBack(
                true,
                typeof(CollectableType),
                nameof(CollectableType.Numismatic),
                CultureInfo.InvariantCulture);

            Assert.AreEqual(CollectableType.Numismatic, result);
        }

        [TestMethod]
        public void EnumToBooleanConverter_ConvertBack_InvalidParameter_ReturnsDoNothing()
        {
            var converter = new EnumToBooleanConverter();

            var result = converter.ConvertBack(
                true,
                typeof(CollectableType),
                InvalidEnumName,
                CultureInfo.InvariantCulture);

            Assert.AreSame(Binding.DoNothing, result);
        }

        [TestMethod]
        public void EnumToBooleanConverter_ConvertBack_NullParameter_ReturnsDoNothing()
        {
            var converter = new EnumToBooleanConverter();

            var result = converter.ConvertBack(
                true,
                typeof(CollectableType),
                null!,
                CultureInfo.InvariantCulture);

            Assert.AreSame(Binding.DoNothing, result);
        }

        [TestMethod]
        public void EnumToBooleanConverter_ConvertBack_WithNonBooleanValue_ReturnsDoNothing()
        {
            var converter = new EnumToBooleanConverter();

            var result = converter.ConvertBack(
                NonBooleanText,
                typeof(CollectableType),
                nameof(CollectableType.Bullion),
                CultureInfo.InvariantCulture);

            Assert.AreSame(Binding.DoNothing, result);
        }

        [TestMethod]
        public void EnumToBooleanConverter_ConvertBack_False_ReturnsDoNothing()
        {
            var converter = new EnumToBooleanConverter();

            var result = converter.ConvertBack(
                false,
                typeof(CollectableType),
                nameof(CollectableType.Bullion),
                CultureInfo.InvariantCulture);

            Assert.AreSame(Binding.DoNothing, result);
        }

        [TestMethod]
        public void MetalTypeToLabelConverter_ConvertBack_ReturnsDoNothing()
        {
            var converter = new MetalTypeToLabelConverter();

            var result = converter.ConvertBack(
                nameof(MetalType.Gold),
                typeof(MetalType),
                null!,
                CultureInfo.InvariantCulture);

            Assert.AreSame(Binding.DoNothing, result);
        }

        [TestMethod]
        public void CollectableTypeToLabelConverter_ConvertBack_ReturnsDoNothing()
        {
            var converter = new CollectableTypeToLabelConverter();

            var result = converter.ConvertBack(
                nameof(CollectableType.Bullion),
                typeof(CollectableType),
                null!,
                CultureInfo.InvariantCulture);

            Assert.AreSame(Binding.DoNothing, result);
        }

        [TestMethod]
        public void TaxFreeStatusConverter_ConvertBack_ReturnsDoNothing()
        {
            var converter = new TaxFreeStatusConverter();

            var result = converter.ConvertBack(
                ArbitraryDisplayText,
                typeof(string),
                null!,
                CultureInfo.InvariantCulture);

            Assert.AreSame(Binding.DoNothing, result);
        }

        private static void EnsureWpfApp()
        {
            if (Application.Current is not null)
                return;

            _ = new Application
            {
                ShutdownMode = ShutdownMode.OnExplicitShutdown
            };
        }
    }
}