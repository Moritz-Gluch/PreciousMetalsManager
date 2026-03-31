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

            if (dialog.ShowDialog() != true)
                return null;

            var vm = dialog.ViewModel;
            return new PriceEditResult(
                vm.GoldPrice,
                vm.SilverPrice,
                vm.PlatinumPrice,
                vm.PalladiumPrice,
                vm.BroncePrice);
        }

        private static Window? GetOwnerWindow()
            => Application.Current?.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive);
    }
}