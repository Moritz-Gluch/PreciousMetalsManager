using System.Linq;
using System.Windows;
using PreciousMetalsManager.ViewModels;
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
            var dialogViewModel = new EditPricesDialogViewModel(
                goldPrice,
                silverPrice,
                platinumPrice,
                palladiumPrice,
                broncePrice,
                priceUnit);

            var dialog = new EditPricesDialog(dialogViewModel)
            {
                Owner = GetOwnerWindow()
            };

            if (dialog.ShowDialog() != true)
                return null;

            return new PriceEditResult(
                dialogViewModel.GoldPrice,
                dialogViewModel.SilverPrice,
                dialogViewModel.PlatinumPrice,
                dialogViewModel.PalladiumPrice,
                dialogViewModel.BroncePrice);
        }

        private static Window? GetOwnerWindow()
            => Application.Current?.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive);
    }
}