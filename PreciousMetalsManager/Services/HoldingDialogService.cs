using System.Globalization;
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
            var dialog = new HoldingDialog
            {
                DataContext = viewModel,
                Owner = GetOwnerWindow()
            };

            if (dialog.ShowDialog() == true && dialog.NewHolding is { } holding)
                return new HoldingDialogResult(true, holding, dialog.AddAnotherRequested);

            return HoldingDialogResult.Cancelled;
        }

        public HoldingDialogResult ShowEditDialog(ViewModel viewModel, MetalHolding holding)
        {
            var dialog = new HoldingDialog
            {
                DataContext = viewModel,
                Owner = GetOwnerWindow(),
                IsEditMode = true
            };

            dialog.MetalTypeComboBox.SelectedItem = holding.MetalType;
            dialog.FormTextBox.Text = holding.Form;
            dialog.PurityComboBox.Text = holding.Purity.ToString(CultureInfo.InvariantCulture);
            dialog.WeightTextBox.Text = holding.Weight.ToString(CultureInfo.InvariantCulture);
            dialog.QuantityTextBox.Text = holding.Quantity.ToString(CultureInfo.InvariantCulture);
            dialog.PurchasePriceTextBox.Text = holding.PurchasePrice.ToString(CultureInfo.InvariantCulture);
            dialog.PurchaseDatePicker.SelectedDate = holding.PurchaseDate;
            dialog.SelectedCollectableType = holding.CollectableType;

            if (dialog.ShowDialog() == true && dialog.NewHolding is { } editedHolding)
                return new HoldingDialogResult(true, editedHolding, false);

            return HoldingDialogResult.Cancelled;
        }

        private static Window? GetOwnerWindow()
            => Application.Current?.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive);
    }
}