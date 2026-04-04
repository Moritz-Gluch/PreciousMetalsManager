using Microsoft.VisualStudio.TestTools.UnitTesting;
using PreciousMetalsManager.Domain;
using PreciousMetalsManager.Models;
using System;
using System.Linq;

namespace PreciousMetalsManager.Tests
{
    [TestClass]
    public sealed class DomainReferenceDataTest
    {
        [TestMethod]
        public void TryGetMetalLabelResourceKey_ReturnsExpectedKey_ForKnownMetalType()
        {
            var found = DomainReferenceData.TryGetMetalLabelResourceKey(MetalType.Platinum, out var key);

            Assert.IsTrue(found);
            Assert.AreEqual("Lbl_Platinum", key);
        }

        [TestMethod]
        public void TryGetCollectableLabelResourceKey_ReturnsExpectedKey_ForKnownCollectableType()
        {
            var found = DomainReferenceData.TryGetCollectableLabelResourceKey(CollectableType.Numismatic, out var key);

            Assert.IsTrue(found);
            Assert.AreEqual("CollectableType_Numismatic", key);
        }

        [TestMethod]
        public void TryGetMetalLabelResourceKey_ReturnsResourceKey_ForEveryDefinedMetalType()
        {
            foreach (var metalType in Enum.GetValues(typeof(MetalType)).Cast<MetalType>())
            {
                var found = DomainReferenceData.TryGetMetalLabelResourceKey(metalType, out var key);

                Assert.IsTrue(found, $"No label resource key found for metal type '{metalType}'.");
                Assert.IsFalse(string.IsNullOrWhiteSpace(key), $"Resource key for metal type '{metalType}' is empty.");
            }
        }

        [TestMethod]
        public void TryGetCollectableLabelResourceKey_ReturnsResourceKey_ForEveryDefinedCollectableType()
        {
            foreach (var collectableType in Enum.GetValues(typeof(CollectableType)).Cast<CollectableType>())
            {
                var found = DomainReferenceData.TryGetCollectableLabelResourceKey(collectableType, out var key);

                Assert.IsTrue(found, $"No label resource key found for collectable type '{collectableType}'.");
                Assert.IsFalse(string.IsNullOrWhiteSpace(key), $"Resource key for collectable type '{collectableType}' is empty.");
            }
        }

        [TestMethod]
        public void TryGetMetalLabelResourceKey_ReturnsFalse_ForUnknownMetalType()
        {
            var found = DomainReferenceData.TryGetMetalLabelResourceKey((MetalType)999, out var key);

            Assert.IsFalse(found);
            Assert.IsNull(key);
        }

        [TestMethod]
        public void TryGetCollectableLabelResourceKey_ReturnsFalse_ForUnknownCollectableType()
        {
            var found = DomainReferenceData.TryGetCollectableLabelResourceKey((CollectableType)999, out var key);

            Assert.IsFalse(found);
            Assert.IsNull(key);
        }
    }
}