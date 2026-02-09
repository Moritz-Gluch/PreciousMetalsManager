using PreciousMetalsManager.ViewModels;
using PreciousMetalsManager.Views;
using System.Windows;

namespace PreciousMetalsManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ViewModel();
        }

        private static string L(string key)
            => Application.Current?.TryFindResource(key) as string ?? key;

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new HoldingDialog();
            if (addWindow.ShowDialog() == true && DataContext is ViewModel vm && addWindow.NewHolding is { } newHolding)
            {
                vm.AddHolding(newHolding);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ViewModel vm)
                return;

            if (MainDataGrid.SelectedItem is Models.MetalHolding selected)
            {
                var editWindow = new HoldingDialog();

                // Load presets
                editWindow.MetalTypeComboBox.SelectedItem = selected.MetalType;
                editWindow.FormTextBox.Text = selected.Form;
                editWindow.PurityTextBox.Text = selected.Purity.ToString();
                editWindow.WeightTextBox.Text = selected.Weight.ToString();
                editWindow.QuantityTextBox.Text = selected.Quantity.ToString();
                editWindow.PurchasePriceTextBox.Text = selected.PurchasePrice.ToString();
                editWindow.PurchaseDatePicker.SelectedDate = selected.PurchaseDate;
                editWindow.SelectedCollectableType = selected.CollectableType;

                if (editWindow.ShowDialog() == true && editWindow.NewHolding is { } edited)
                {
                    // Adopt changes
                    selected.MetalType = edited.MetalType;
                    selected.Form = edited.Form;
                    selected.Purity = edited.Purity;
                    selected.Weight = edited.Weight;
                    selected.Quantity = edited.Quantity;
                    selected.PurchasePrice = edited.PurchasePrice;
                    selected.PurchaseDate = edited.PurchaseDate;

                    vm.UpdateHolding(selected);

                    MainDataGrid.Items.Refresh();
                }
            }
            else
            {
                MessageBox.Show(L("Msg_SelectHoldingToEdit"));
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ViewModel vm)
                return;

            if (MainDataGrid.SelectedItem is Models.MetalHolding selected)
            {
                // Confirmation box
                var result = MessageBox.Show(
                    L("Msg_ConfirmDeleteText"),
                    L("Msg_ConfirmDeleteTitle"),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.Yes)
                {
                    vm.DeleteHolding(selected);
                }
            }
            else
            {
                MessageBox.Show(L("Msg_SelectHoldingToDelete"));
            }
        }

        private void EditPricesButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ViewModel vm)
                return;

            var dlg = new Views.EditPricesDialog(vm.GoldPrice, vm.SilverPrice, vm.PlatinumPrice, vm.PalladiumPrice, vm.BroncePrice)
            {
                Owner = this
            };

            if (dlg.ShowDialog() == true)
            {
                vm.GoldPrice = dlg.GoldPrice;
                vm.SilverPrice = dlg.SilverPrice;
                vm.PlatinumPrice = dlg.PlatinumPrice;
                vm.PalladiumPrice = dlg.PalladiumPrice;
                vm.BroncePrice = dlg.BroncePrice;
            }
        }

        private void LanguageButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModel vm)
            {
                vm.ToggleLanguage();

                // Enforces Refresh of the dropdown entries (necessary for language change)
                MetalTypeFilterComboBox.Items.Refresh();
            }
        }
    }
}
