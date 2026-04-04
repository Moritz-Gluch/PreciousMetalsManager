using Microsoft.VisualStudio.TestTools.UnitTesting;
using PreciousMetalsManager.Services;
using System.Threading.Tasks;

namespace PreciousMetalsManager.Tests
{
    [TestClass]
    public sealed class MetalPriceApiServiceTest
    {
        [TestMethod]
        public async Task FetchMetalPricesAsync_ReturnsNull_WhenRawJsonIsNull()
        {
            var service = new StubMetalPriceApiService
            {
                RawJsonToReturn = null
            };

            var result = await service.FetchMetalPricesAsync();

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task FetchMetalPricesAsync_ReturnsDeserializedResponse_WhenJsonIsValid()
        {
            var service = new StubMetalPriceApiService
            {
                RawJsonToReturn =
                    """
                    {
                      "gold_eur": 3110.0,
                      "silber_eur": 62.2,
                      "platin_eur": 155.5,
                      "palladium_eur": 62.2,
                      "timestamp": 1710000000
                    }
                    """
            };

            var result = await service.FetchMetalPricesAsync();

            Assert.IsNotNull(result);
            Assert.AreEqual(3110.0m, result.GoldEur);
            Assert.AreEqual(62.2m, result.SilverEur);
            Assert.AreEqual(155.5m, result.PlatinumEur);
            Assert.AreEqual(62.2m, result.PalladiumEur);
            Assert.AreEqual(1710000000L, result.Timestamp);
        }

        [TestMethod]
        public async Task FetchMetalPricesAsync_ReturnsNull_WhenJsonIsInvalid()
        {
            var service = new StubMetalPriceApiService
            {
                RawJsonToReturn = "this is not valid json"
            };

            var result = await service.FetchMetalPricesAsync();

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task FetchMetalPricesAsync_ReturnsObject_WhenJsonIsValidButIncomplete()
        {
            var service = new StubMetalPriceApiService
            {
                RawJsonToReturn =
                    """
                    {
                      "gold_eur": 1234.5
                    }
                    """
            };

            var result = await service.FetchMetalPricesAsync();

            Assert.IsNotNull(result);
            Assert.AreEqual(1234.5m, result.GoldEur);
            Assert.AreEqual(0m, result.SilverEur);
            Assert.AreEqual(0m, result.PlatinumEur);
            Assert.AreEqual(0m, result.PalladiumEur);
            Assert.AreEqual(0L, result.Timestamp);
        }

        private sealed class StubMetalPriceApiService : MetalPriceApiService
        {
            public string? RawJsonToReturn { get; set; }

            public override Task<string?> FetchMetalPricesRawAsync()
                => Task.FromResult(RawJsonToReturn);
        }
    }
}
