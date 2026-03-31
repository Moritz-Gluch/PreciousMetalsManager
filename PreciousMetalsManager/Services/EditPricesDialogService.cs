using System.Linq;
using System.Windows;
using PreciousMetalsManager.Views;

namespace PreciousMetalsManager.Services
{
    public sealed class EditPricesDialogService : IEditPricesDialogService
    {
        public PriceEditResult? ShowEditPricesDialog(
            decimal goldPrice,
            decimal silverPrice,
            decimal platinumPrice,
            decimal palladiumPrice,
            decimal broncePrice,
            string priceUnit)
        {
            var dialog = new EditPricesDialog(
                goldPrice,
                silverPrice,
                platinumPrice,
                palladiumPrice,
                broncePrice,
                priceUnit)
            {
                Owner = GetOwnerWindow()
            };

            return dialog.ShowDialog() == true
                ? new PriceEditResult(
                    dialog.GoldPrice,
                    dialog.SilverPrice,
                    dialog.PlatinumPrice,
                    dialog.PalladiumPrice,
                    dialog.BroncePrice)
                : null;
        }

        private static Window? GetOwnerWindow()
            => Application.Current?.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive);
    }
}