using Microsoft.VisualStudio.TestTools.UnitTesting;
using PreciousMetalsManager.Services;
using System.Windows;

namespace PreciousMetalsManager.Tests
{
    [STATestClass]
    public sealed class LanguageServiceTest
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            EnsureWpfApp();
        }

        [TestMethod]
        public void CurrentLanguage_ReturnsAppCurrentLanguage()
        {
            App.SetLanguage("en");
            var service = new LanguageService();

            Assert.AreEqual("en", service.CurrentLanguage);

            App.SetLanguage("de");

            Assert.AreEqual("de", service.CurrentLanguage);
        }

        [TestMethod]
        public void ToggleLanguage_SwitchesFromEnToDe()
        {
            App.SetLanguage("en");
            var service = new LanguageService();

            service.ToggleLanguage();

            Assert.AreEqual("de", App.CurrentLanguage);
            Assert.AreEqual("de", service.CurrentLanguage);
        }

        [TestMethod]
        public void ToggleLanguage_SwitchesFromDeToEn()
        {
            App.SetLanguage("de");
            var service = new LanguageService();

            service.ToggleLanguage();

            Assert.AreEqual("en", App.CurrentLanguage);
            Assert.AreEqual("en", service.CurrentLanguage);
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

    public sealed class LanguageService : ILanguageService
    {
        public string CurrentLanguage => App.CurrentLanguage;

        public void ToggleLanguage()
            => App.SetLanguage(App.CurrentLanguage == "en" ? "de" : "en");
    }
}
