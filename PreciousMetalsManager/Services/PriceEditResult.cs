namespace PreciousMetalsManager.Services
{
    public sealed class PriceEditResult
    {
        public PriceEditResult(decimal goldPrice, decimal silverPrice, decimal platinumPrice, decimal palladiumPrice, decimal broncePrice)
        {
            GoldPrice = goldPrice;
            SilverPrice = silverPrice;
            PlatinumPrice = platinumPrice;
            PalladiumPrice = palladiumPrice;
            BroncePrice = broncePrice;
        }

        public decimal GoldPrice { get; }
        public decimal SilverPrice { get; }
        public decimal PlatinumPrice { get; }
        public decimal PalladiumPrice { get; }
        public decimal BroncePrice { get; }
    }
}