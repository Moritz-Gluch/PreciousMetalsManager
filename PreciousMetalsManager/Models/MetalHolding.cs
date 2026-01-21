using System;

namespace PreciousMetalsManager.Models
{
    public enum MetalType
    {
        Gold,
        Silver,
        Bronce,
        Platinum,
        Palladium
    }

    public class MetalHolding
    {
        public MetalType MetalType { get; set; }

        public string Form { get; set; } = string.Empty;

        public decimal Purity { get; set; }

        public decimal Weight { get; set; }

        public int Quantity { get; set; }

        public decimal PurchasePrice { get; set; }

        public DateTime PurchaseDate { get; set; }

        // Will be calculated later (STORY-03)
        public decimal CurrentValue { get; set; }
        public decimal TotalValue { get; set; }
    }
}
