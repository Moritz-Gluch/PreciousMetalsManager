using Microsoft.VisualStudio.TestTools.UnitTesting;
using PreciousMetalsManager.Converters;
using PreciousMetalsManager.Domain;
using PreciousMetalsManager.Models;
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using System.Xml;

namespace PreciousMetalsManager.Tests
{
    [STATestClass]
    public sealed class LocalizationTest
    {
        private const string LocalizationFolder = "Resources/Localization/";

        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            EnsureWpfApp();
        }

        [TestMethod]
        public void LocalizationDictionaries_ShouldContainSameKeys()
        {
            var en = Load($"{LocalizationFolder}Localization.en.xaml");
            var de = Load($"{LocalizationFolder}Localization.de.xaml");

            var enKeys = GetStringKeys(en);
            var deKeys = GetStringKeys(de);

            CollectionAssert.AreEquivalent(enKeys, deKeys, "DE/EN ResourceDictionary keys are not identical.");
        }

        [TestMethod]
        public void Localization_AllStringResources_ShouldBeNonEmpty()
        {
            var en = Load($"{LocalizationFolder}Localization.en.xaml");
            var de = Load($"{LocalizationFolder}Localization.de.xaml");

            AssertNoNullOrWhitespaceStrings(en, "en");
            AssertNoNullOrWhitespaceStrings(de, "de");
        }

        [TestMethod]
        public void SetLanguage_ShouldThrow_ForUnsupportedLanguage()
        {
            AssertThrows<NotSupportedException>(() => App.SetLanguage("fr"));
        }

        [TestMethod]
        public void SetLanguage_ShouldThrow_ForNullOrWhitespace()
        {
            AssertThrows<ArgumentException>(() => App.SetLanguage(null!));
            AssertThrows<ArgumentException>(() => App.SetLanguage(string.Empty));
            AssertThrows<ArgumentException>(() => App.SetLanguage("   "));
        }

        [TestMethod]
        public void TaxFreeStatusConverter_ShouldReturnEmpty_ForNonHoldingInput()
        {
            App.SetLanguage("en");

            var converter = new TaxFreeStatusConverter();
            var result = converter.Convert("not-a-holding", typeof(string), null!, CultureInfo.InvariantCulture);

            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void SetLanguage_ShouldBeCaseInsensitive()
        {
            App.SetLanguage("EN");
            Assert.AreEqual("en", App.CurrentLanguage);

            App.SetLanguage("De");
            Assert.AreEqual("de", App.CurrentLanguage);
        }

        [TestMethod]
        public void SetLanguage_ShouldNotAccumulateLocalizationDictionaries()
        {
            var merged = Application.Current.Resources.MergedDictionaries;

            App.SetLanguage("en");
            var countAfterEn = CountLocalizationDictionaries(merged);
            Assert.AreEqual(1, countAfterEn, "After SetLanguage(en) exactly one localization dictionary should be merged.");

            App.SetLanguage("de");
            var countAfterDe = CountLocalizationDictionaries(merged);
            Assert.AreEqual(1, countAfterDe, "After SetLanguage(de) exactly one localization dictionary should be merged.");
        }

        [TestMethod]
        public void SetLanguage_ShouldPersistLanguageSelection()
        {
            var tempSettingsDir = Path.Combine(Path.GetTempPath(), "PreciousMetalsManager.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempSettingsDir);

            try
            {
                var store = new PreciousMetalsManager.Services.LanguagePreferenceStore(tempSettingsDir);

                PreciousMetalsManager.App.SetLanguagePreferenceStoreForTests(store);

                App.SetLanguage("de");
                Assert.AreEqual("de", store.TryLoad());

                App.SetLanguage("en");
                Assert.AreEqual("en", store.TryLoad());
            }
            finally
            {
                if (Directory.Exists(tempSettingsDir))
                    Directory.Delete(tempSettingsDir, recursive: true);

                PreciousMetalsManager.App.ResetLanguagePreferenceStoreForTests();
            }
        }

        [TestMethod]
        public void SetLanguage_ShouldResolveAllLocalizationKeysAtRuntime_En()
        {
            var keys = GetStringKeys(Load($"{LocalizationFolder}Localization.en.xaml"));

            App.SetLanguage("en");
            AssertRuntimeResourcesResolve("en", keys);
        }

        [TestMethod]
        public void SetLanguage_ShouldResolveAllLocalizationKeysAtRuntime_De()
        {
            var keys = GetStringKeys(Load($"{LocalizationFolder}Localization.de.xaml"));

            App.SetLanguage("de");
            AssertRuntimeResourcesResolve("de", keys);
        }

        [TestMethod]
        public void DomainReferenceData_LabelResourceKeys_ShouldExistInBothDictionaries()
        {
            var en = Load($"{LocalizationFolder}Localization.en.xaml");
            var de = Load($"{LocalizationFolder}Localization.de.xaml");

            var labelKeys = DomainReferenceData.PreciousMetals.LabelResourceKeys.Values
                .Concat(DomainReferenceData.Collectables.LabelResourceKeys.Values)
                .Distinct(StringComparer.Ordinal);

            foreach (var key in labelKeys)
            {
                Assert.IsTrue(en.Contains(key), $"Missing EN domain label resource key: {key}");
                Assert.IsTrue(de.Contains(key), $"Missing DE domain label resource key: {key}");
            }
        }

        [TestMethod]
        public void MetalTypeToLabelConverter_ShouldUseLocalizedResourceLabels_ForEnumNameStrings()
        {
            App.SetLanguage("de");

            var converter = new MetalTypeToLabelConverter();

            Assert.AreEqual(GetExpectedMetalLabel(MetalType.Gold), converter.Convert(nameof(MetalType.Gold), typeof(string), null!, CultureInfo.InvariantCulture));
            Assert.AreEqual(GetExpectedMetalLabel(MetalType.Silver), converter.Convert(nameof(MetalType.Silver), typeof(string), null!, CultureInfo.InvariantCulture));
            Assert.AreEqual(GetExpectedMetalLabel(MetalType.Platinum), converter.Convert(nameof(MetalType.Platinum), typeof(string), null!, CultureInfo.InvariantCulture));
            Assert.AreEqual(GetExpectedMetalLabel(MetalType.Palladium), converter.Convert(nameof(MetalType.Palladium), typeof(string), null!, CultureInfo.InvariantCulture));
            Assert.AreEqual(GetExpectedMetalLabel(MetalType.Bronce), converter.Convert(nameof(MetalType.Bronce), typeof(string), null!, CultureInfo.InvariantCulture));
        }

        [TestMethod]
        public void CollectableTypeToLabelConverter_ShouldUseLocalizedResourceLabels_ForEnumNameStrings()
        {
            App.SetLanguage("de");

            var converter = new CollectableTypeToLabelConverter();

            Assert.AreEqual(GetExpectedCollectableLabel(CollectableType.Bullion), converter.Convert(nameof(CollectableType.Bullion), typeof(string), null!, CultureInfo.InvariantCulture));
            Assert.AreEqual(GetExpectedCollectableLabel(CollectableType.SemiNumismatic), converter.Convert(nameof(CollectableType.SemiNumismatic), typeof(string), null!, CultureInfo.InvariantCulture));
            Assert.AreEqual(GetExpectedCollectableLabel(CollectableType.Numismatic), converter.Convert(nameof(CollectableType.Numismatic), typeof(string), null!, CultureInfo.InvariantCulture));
        }

        [TestMethod]
        public void TaxFreeStatusConverter_ShouldReturnLocalizedYes_OnExactTaxFreeBoundary()
        {
            App.SetLanguage("en");

            var converter = new TaxFreeStatusConverter();
            var holding = new MetalHolding
            {
                PurchaseDate = DateTime.Today.AddYears(-DomainReferenceData.Tax.TaxFreeHoldingPeriodYears)
            };

            var result = converter.Convert(holding, typeof(string), null!, CultureInfo.InvariantCulture);

            Assert.AreEqual(GetRequiredStringResource("TaxFreeStatus_Yes"), result);
        }

        [TestMethod]
        public void TaxFreeStatusConverter_ShouldReturnLocalizedDaysLeft_WhenHoldingIsNotTaxFree()
        {
            App.SetLanguage("en");

            var converter = new TaxFreeStatusConverter();
            var holding = new MetalHolding
            {
                PurchaseDate = DateTime.Today.AddMonths(-6)
            };

            var result = converter.Convert(holding, typeof(string), null!, CultureInfo.InvariantCulture) as string;

            Assert.IsNotNull(result);

            var expected = $"{holding.TaxFreeDaysLeft} {GetRequiredStringResource("TaxFreeStatus_DaysLeft")}";
            Assert.AreEqual(expected, result);
        }

        private static string GetExpectedMetalLabel(MetalType metalType)
        {
            Assert.IsTrue(
                DomainReferenceData.TryGetMetalLabelResourceKey(metalType, out var key),
                $"Missing metal label resource key mapping for {metalType}.");

            return GetExpectedLabel(key);
        }

        private static string GetExpectedCollectableLabel(CollectableType collectableType)
        {
            Assert.IsTrue(
                DomainReferenceData.TryGetCollectableLabelResourceKey(collectableType, out var key),
                $"Missing collectable label resource key mapping for {collectableType}.");

            return GetExpectedLabel(key);
        }

        private static string GetExpectedLabel(string resourceKey)
            => GetRequiredStringResource(resourceKey).TrimEnd().TrimEnd(':');

        private static string GetRequiredStringResource(string key)
        {
            var value = Application.Current.TryFindResource(key) as string;

            Assert.IsFalse(string.IsNullOrWhiteSpace(value), $"Runtime resource missing or empty: {key}");

            return value!;
        }

        private static void AssertRuntimeResourcesResolve(string languageCode, params string[] keys)
        {
            App.SetLanguage(languageCode);

            foreach (var key in keys)
            {
                var value = Application.Current.TryFindResource(key) as string;
                Assert.IsFalse(string.IsNullOrWhiteSpace(value), $"[{languageCode}] Runtime resource missing or empty: {key}");
            }
        }

        private static ResourceDictionary Load(string relativePathFromProjectRoot)
        {
            var baseDir = AppContext.BaseDirectory;

            var fullPath = Path.GetFullPath(Path.Combine(
                baseDir,
                "..", "..", "..", "..",
                "PreciousMetalsManager",
                relativePathFromProjectRoot.Replace('/', Path.DirectorySeparatorChar)));

            if (!File.Exists(fullPath))
                Assert.Fail($"Localization file not found: {fullPath}");

            using var stream = File.OpenRead(fullPath);
            using var xmlReader = XmlReader.Create(stream);

            return (ResourceDictionary)XamlReader.Load(xmlReader);
        }

        private static string[] GetStringKeys(ResourceDictionary dict)
            => dict.Keys.OfType<string>()
                .Where(k => dict[k] is string)
                .OrderBy(k => k, StringComparer.Ordinal)
                .ToArray();

        private static void AssertNoNullOrWhitespaceStrings(ResourceDictionary dict, string languageCode)
        {
            foreach (DictionaryEntry entry in dict)
            {
                if (entry.Key is not string key)
                    continue;

                if (entry.Value is not string value)
                    continue;

                Assert.IsFalse(string.IsNullOrWhiteSpace(value), $"[{languageCode}] String is empty: {key}");
            }
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

        private static int CountLocalizationDictionaries(System.Collections.ObjectModel.Collection<ResourceDictionary> merged)
            => merged.Count(d => d.Source?.OriginalString?.IndexOf("/Resources/Localization/Localization.", StringComparison.OrdinalIgnoreCase) >= 0);

        private static void AssertThrows<TException>(Action action) where TException : Exception
        {
            try
            {
                action();
                Assert.Fail($"Expected exception {typeof(TException).Name} was not thrown.");
            }
            catch (TException)
            {
                // expected
            }
        }
    }
}