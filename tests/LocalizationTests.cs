using Microsoft.VisualStudio.TestTools.UnitTesting;
using PreciousMetalsManager.Models;
using PreciousMetalsManager.ViewModels;
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
    public sealed class LocalizationTests
    {
        private const string LocalizationFolder = "Resources/Localization/";
        private string _testDbPath = null!;

        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            EnsureWpfApp();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _testDbPath = Path.Combine(Path.GetTempPath(), $"test_holdings_{Guid.NewGuid()}.db");
        }

        private ViewModel? _vm;

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
        public void SetLanguage_ShouldSwitchStringResources()
        {
            App.SetLanguage("en");
            Assert.AreEqual("Add", Application.Current.TryFindResource("AddButton") as string);

            App.SetLanguage("de");
            Assert.AreEqual("Hinzufügen", Application.Current.TryFindResource("AddButton") as string);
        }

        [TestMethod]
        public void SetLanguage_ShouldThrow_ForUnsupportedLanguage()
        {
            try
            {
                App.SetLanguage("fr");
                Assert.Fail("Expected NotSupportedException was not thrown.");
            }
            catch (NotSupportedException)
            {
                // expected
            }
        }

        [TestMethod]
        public void SetLanguage_ShouldThrow_ForNullOrWhitespace()
        {
            AssertThrows<ArgumentException>(() => App.SetLanguage(null!));
            AssertThrows<ArgumentException>(() => App.SetLanguage(string.Empty));
            AssertThrows<ArgumentException>(() => App.SetLanguage("   "));
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
        public void SetLanguage_ShouldProvideExpectedResources_ForMultipleKeys()
        {
            App.SetLanguage("en");
            Assert.AreEqual("Precious Metals Manager", Application.Current.TryFindResource("MainWindow_Title") as string);
            Assert.AreEqual("All", Application.Current.TryFindResource("Filter_All") as string);
            Assert.AreEqual("Confirm Delete", Application.Current.TryFindResource("Msg_ConfirmDeleteTitle") as string);

            App.SetLanguage("de");
            Assert.AreEqual("Edelmetall-Manager", Application.Current.TryFindResource("MainWindow_Title") as string);
            Assert.AreEqual("Alle", Application.Current.TryFindResource("Filter_All") as string);
            Assert.AreEqual("Löschen bestätigen", Application.Current.TryFindResource("Msg_ConfirmDeleteTitle") as string);
        }

        [TestMethod]
        public void MetalTypeToLabelConverter_ShouldReturnLocalizedLabel_WithoutColon()
        {
            App.SetLanguage("de");

            var converter = new MetalTypeToLabelConverter();
            var result = converter.Convert(MetalType.Bronce, typeof(string), null!, CultureInfo.InvariantCulture);

            Assert.AreEqual("Bronze", result);
        }

        [TestMethod]
        public void MetalTypeToLabelConverter_ShouldPassThrough_FilterAll_String()
        {
            App.SetLanguage("de");

            var converter = new MetalTypeToLabelConverter();
            var filterAll = Application.Current.TryFindResource("Filter_All") as string;

            Assert.IsNotNull(filterAll);

            var result = converter.Convert(filterAll!, typeof(string), null!, CultureInfo.InvariantCulture);
            Assert.AreEqual(filterAll, result);
        }

        [TestMethod]
        public void MetalTypeToLabelConverter_ShouldHandleNull()
        {
            App.SetLanguage("en");

            var converter = new MetalTypeToLabelConverter();
            var result = converter.Convert(null!, typeof(string), null!, CultureInfo.InvariantCulture);

            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void ViewModel_ToggleLanguage_ShouldSwitch_AppCurrentLanguage()
        {
            App.SetLanguage("en");
            _vm = CreateTestViewModel();
            _vm.ToggleLanguage();
            Assert.AreEqual("de", App.CurrentLanguage);
        }

        [TestMethod]
        public void ViewModel_MetalTypeFilterOptions_FirstItem_ShouldMatchLocalizedFilterAll()
        {
            App.SetLanguage("de");
            _vm = CreateTestViewModel();
            var firstDe = _vm.MetalTypeFilterOptions.Cast<object>().First() as string;
            Assert.AreEqual("Alle", firstDe);

            App.SetLanguage("en");
            _vm = CreateTestViewModel();
            var firstEn = _vm.MetalTypeFilterOptions.Cast<object>().First() as string;
            Assert.AreEqual("All", firstEn);
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

        private ViewModel CreateTestViewModel()
        {
            var storage = new PreciousMetalsManager.Services.LocalStorageService(_testDbPath);
            return new ViewModel(storage);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _vm = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            System.Threading.Thread.Sleep(100);

            if (File.Exists(_testDbPath))
                File.Delete(_testDbPath);
        }
    }
}