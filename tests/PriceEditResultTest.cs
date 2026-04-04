using Microsoft.VisualStudio.TestTools.UnitTesting;
using PreciousMetalsManager.Services;

namespace PreciousMetalsManager.Tests
{
    [TestClass]
    public sealed class PriceEditResultTest
    {
        [TestMethod]
        public void Constructor_SetsAllProperties()
        {
            var result = new PriceEditResult(100m, 2m, 5m, 3m, 1m);

            Assert.AreEqual(100m, result.GoldPrice);
            Assert.AreEqual(2m, result.SilverPrice);
            Assert.AreEqual(5m, result.PlatinumPrice);
            Assert.AreEqual(3m, result.PalladiumPrice);
            Assert.AreEqual(1m, result.BroncePrice);
        }
    }
}