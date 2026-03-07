using PreciousMetalsManager.ViewModels;
using PreciousMetalsManager.Views;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

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
            if (DataContext is not ViewModel vm)
                return;

            var keepAdding = true;
            while (keepAdding)
            {
                var addWindow = new HoldingDialog
                {
                    DataContext = DataContext,
                    Owner = this
                };

                if (addWindow.ShowDialog() == true && addWindow.NewHolding is { } newHolding)
                {
                    vm.AddHolding(newHolding);
                    keepAdding = addWindow.AddAnotherRequested;
                }
                else
                {
                    keepAdding = false;
                }
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ViewModel vm)
                return;

            if (MainDataGrid.SelectedItem is Models.MetalHolding selected)
            {
                var editWindow = new HoldingDialog
                {
                    DataContext = DataContext,
                    Owner = this,
                    IsEditMode = true
                };

                // Load presets
                editWindow.MetalTypeComboBox.SelectedItem = selected.MetalType;
                editWindow.FormTextBox.Text = selected.Form;
                editWindow.PurityComboBox.Text = selected.Purity.ToString();
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
                    selected.CollectableType = edited.CollectableType; 

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

            var dlg = new EditPricesDialog(
                vm.GoldPrice,
                vm.SilverPrice,
                vm.PlatinumPrice,
                vm.PalladiumPrice,
                vm.BroncePrice,
                vm.PriceUnit 
            )
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

            RecalculateDataGridColumnWidths();
        }

        // Neccessary workaround to prevent column width issues after language change 
        private void RecalculateDataGridColumnWidths()
        {
            Dispatcher.InvokeAsync(() =>
            {
                if (MainDataGrid?.Columns == null || MainDataGrid.Columns.Count == 0)
                    return;

                // Temporarily disable virtualization to ensure proper layout recalculation
                var oldEnableRowVirt = VirtualizingPanel.GetIsVirtualizing(MainDataGrid);
                var oldVirtMode = VirtualizingPanel.GetVirtualizationMode(MainDataGrid);

                VirtualizingPanel.SetIsVirtualizing(MainDataGrid, false);

                // Force DataGrid to re-evaluate column widths by resetting the ItemsSource
                var oldItemsSource = MainDataGrid.ItemsSource;
                MainDataGrid.ItemsSource = null;
                MainDataGrid.UpdateLayout();

                MainDataGrid.ItemsSource = oldItemsSource;

                // Reset column widths to auto to recalculate based on new language
                foreach (var col in MainDataGrid.Columns)
                {
                    col.Width = new DataGridLength(0);
                    col.Width = DataGridLength.Auto;
                }

                // Set "Form" column to star to fill remaining space
                var formCol = MainDataGrid.Columns
                    .OfType<DataGridTextColumn>()
                    .FirstOrDefault(c => c.Binding is System.Windows.Data.Binding b && b.Path?.Path == "Form");

                if (formCol != null)
                    formCol.Width = new DataGridLength(1, DataGridLengthUnitType.Star);

                MainDataGrid.UpdateLayout();

                // Restore virtualization settings
                VirtualizingPanel.SetIsVirtualizing(MainDataGrid, oldEnableRowVirt);
                VirtualizingPanel.SetVirtualizationMode(MainDataGrid, oldVirtMode);
            }, 
            
            DispatcherPriority.Background);
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.ContextMenu != null)
            {
                button.ContextMenu.PlacementTarget = button;
                button.ContextMenu.IsOpen = true;
            }
        }
    }
}
