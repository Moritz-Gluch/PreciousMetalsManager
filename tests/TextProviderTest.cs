using Microsoft.VisualStudio.TestTools.UnitTesting;
using PreciousMetalsManager.Services;
using System.Windows;

namespace PreciousMetalsManager.Tests
{
    [STATestClass]
    public sealed class TextProviderTest
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            EnsureWpfApp();
        }

        [TestMethod]
        public void GetString_ReturnsResourceValue_WhenKeyExists()
        {
            Application.Current.Resources["Test_Key"] = "Test Value";
            var provider = new TextProvider();

            var result = provider.GetString("Test_Key");

            Assert.AreEqual("Test Value", result);
        }

        [TestMethod]
        public void GetString_ReturnsKey_WhenResourceDoesNotExist()
        {
            var provider = new TextProvider();

            var result = provider.GetString("Missing_Key");

            Assert.AreEqual("Missing_Key", result);
        }

        [TestMethod]
        public void GetString_ReturnsKey_WhenResourceExistsButIsNotString()
        {
            Application.Current.Resources["NonString_Key"] = 123;
            var provider = new TextProvider();

            var result = provider.GetString("NonString_Key");

            Assert.AreEqual("NonString_Key", result);
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
