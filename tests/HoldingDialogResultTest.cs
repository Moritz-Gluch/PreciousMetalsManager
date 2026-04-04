using Microsoft.VisualStudio.TestTools.UnitTesting;
using PreciousMetalsManager.Models;
using PreciousMetalsManager.Services;

namespace PreciousMetalsManager.Tests
{
    [TestClass]
    public sealed class HoldingDialogResultTest
    {
        [TestMethod]
        public void Constructor_SetsAllProperties()
        {
            var holding = new MetalHolding
            {
                Id = 1,
                Form = "Krugerrand"
            };

            var result = new HoldingDialogResult(true, holding, true);

            Assert.IsTrue(result.Accepted);
            Assert.AreSame(holding, result.Holding);
            Assert.IsTrue(result.AddAnotherRequested);
        }

        [TestMethod]
        public void Cancelled_ReturnsExpectedDefaultInstance()
        {
            var result = HoldingDialogResult.Cancelled;

            Assert.IsFalse(result.Accepted);
            Assert.IsNull(result.Holding);
            Assert.IsFalse(result.AddAnotherRequested);
        }

        [TestMethod]
        public void Cancelled_ReturnsSameSingletonInstance()
        {
            var first = HoldingDialogResult.Cancelled;
            var second = HoldingDialogResult.Cancelled;

            Assert.AreSame(first, second);
        }
    }
}