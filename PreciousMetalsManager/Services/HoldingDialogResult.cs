using PreciousMetalsManager.Models;

namespace PreciousMetalsManager.Services
{
    public sealed class HoldingDialogResult
    {
        public static HoldingDialogResult Cancelled { get; } = new(false, null, false);

        public HoldingDialogResult(bool accepted, MetalHolding? holding, bool addAnotherRequested)
        {
            Accepted = accepted;
            Holding = holding;
            AddAnotherRequested = addAnotherRequested;
        }

        public bool Accepted { get; }
        public MetalHolding? Holding { get; }
        public bool AddAnotherRequested { get; }
    }
}