using System.Collections.Generic;
using PreciousMetalsManager.Models;

namespace PreciousMetalsManager.Domain
{
    public static class DomainReferenceData
    {
        public sealed record CurrencyDefinition(string Code, string Symbol);

        public static class Currency
        {
            public const string DefaultCode = "EUR";
            public const string DefaultSymbol = "€";
            public const string PricePerGramUnit = "€/g";
            public const string CurrencyUnit = "(€)";
            public const string SimplifiedCurrencyUnit = "€";
            public const string WeightUnit = "(g)";
            public const string PurityUnit = "(‰)";

            public static IReadOnlyList<CurrencyDefinition> SupportedCurrencies { get; } =
            [
                new(DefaultCode, DefaultSymbol)
            ];
        }

        public static class PreciousMetals
        {
            // A Purity of 999.9 is considered as highest purity (100%)
            public const decimal MaximumFinenessPermille = 999.9m;

            public const decimal MinimumFinenessPermille = 0.1m;
            
            // 1 troy ounce = 31.1g (may be adjusted to the exact value in the future)
            public const decimal RoundedTroyOunceInGrams = 31.1m;

            // Common purities for metals for easy selection in the UI, may be adjusted in the future
            public static IReadOnlyList<decimal> CommonFinenessValues { get; } =
            [
                999.9m,
                925.0m,
                900.0m,
                835.0m,
                800.0m,
                750.0m,
                625.0m
            ];

            public static IReadOnlyDictionary<MetalType, string> LabelResourceKeys { get; } =
                new Dictionary<MetalType, string>
                {
                    [MetalType.Gold] = "Lbl_Gold",
                    [MetalType.Silver] = "Lbl_Silver",
                    [MetalType.Platinum] = "Lbl_Platinum",
                    [MetalType.Palladium] = "Lbl_Palladium",
                    [MetalType.Bronce] = "Lbl_Bronce"
                };
        }

        public static class Collectables
        {
            public static IReadOnlyDictionary<CollectableType, string> LabelResourceKeys { get; } =
                new Dictionary<CollectableType, string>
                {
                    [CollectableType.Bullion] = "CollectableType_Bullion",
                    [CollectableType.SemiNumismatic] = "CollectableType_SemiNumismatic",
                    [CollectableType.Numismatic] = "CollectableType_Numismatic"
                };
        }

        public static class Tax
        {
            public const int TaxFreeHoldingPeriodYears = 1;
        }

        public static bool TryGetMetalLabelResourceKey(MetalType metalType, out string resourceKey)
            => PreciousMetals.LabelResourceKeys.TryGetValue(metalType, out resourceKey!);

        public static bool TryGetCollectableLabelResourceKey(CollectableType collectableType, out string resourceKey)
            => Collectables.LabelResourceKeys.TryGetValue(collectableType, out resourceKey!);
    }
}