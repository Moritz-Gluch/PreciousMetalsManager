using System.Linq;
using System.Windows;
using PreciousMetalsManager.Models;
using PreciousMetalsManager.ViewModels;
using PreciousMetalsManager.Views;

namespace PreciousMetalsManager.Services
{
    public sealed class HoldingDialogService : IHoldingDialogService
    {
        public HoldingDialogResult ShowAddDialog(ViewModel viewModel)
        {
            var dialogViewModel = new HoldingDialogViewModel();

            var dialog = new HoldingDialog(dialogViewModel)
            {
                Owner = GetOwnerWindow()
            };

            if (dialog.ShowDialog() == true && dialogViewModel.CreatedHolding is { } holding)
                return new HoldingDialogResult(true, holding, dialogViewModel.AddAnotherRequested);

            return HoldingDialogResult.Cancelled;
        }

        public HoldingDialogResult ShowEditDialog(ViewModel viewModel, MetalHolding holding)
        {
            var dialogViewModel = new HoldingDialogViewModel(holding);

            var dialog = new HoldingDialog(dialogViewModel)
            {
                Owner = GetOwnerWindow()
            };

            if (dialog.ShowDialog() == true && dialogViewModel.CreatedHolding is { } editedHolding)
                return new HoldingDialogResult(true, editedHolding, false);

            return HoldingDialogResult.Cancelled;
        }

        private static Window? GetOwnerWindow()
            => Application.Current?.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive);
    }
}