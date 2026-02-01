using System;
using System.Text.Json.Serialization;

namespace PreciousMetalsManager.Services
{
    public class MetalPriceApiResponse
    {
        [JsonPropertyName("gold_eur")]
        public decimal GoldEur { get; set; }

        [JsonPropertyName("silber_eur")]
        public decimal SilverEur { get; set; }

        [JsonPropertyName("platin_eur")]
        public decimal PlatinumEur { get; set; }

        [JsonPropertyName("palladium_eur")]
        public decimal PalladiumEur { get; set; }

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }
    }
}
