using System;
using System.Collections.Generic;
using System.Text;

namespace PreciousMetalsManager.Services
{
    public sealed class DetailedExportTexts
    {
        public required string MetalTypeHeader { get; init; }
        public required string FormHeader { get; init; }
        public required string CollectableTypeHeader { get; init; }
        public required string PurityHeader { get; init; }
        public required string WeightHeader { get; init; }
        public required string QuantityHeader { get; init; }
        public required string PurchasePriceHeader { get; init; }
        public required string PurchaseDateHeader { get; init; }

        public required string GoldLabel { get; init; }
        public required string SilverLabel { get; init; }
        public required string BronceLabel { get; init; }
        public required string PlatinumLabel { get; init; }
        public required string PalladiumLabel { get; init; }

        public required string BullionLabel { get; init; }
        public required string SemiNumismaticLabel { get; init; }
        public required string NumismaticLabel { get; init; }
    }
}
