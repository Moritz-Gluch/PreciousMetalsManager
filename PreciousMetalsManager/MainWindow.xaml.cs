using PreciousMetalsManager.ViewModels;
using System;
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

            var viewModel = new ViewModel();
            viewModel.LanguageLayoutRefreshRequested += ViewModel_LanguageLayoutRefreshRequested;
            DataContext = viewModel;
        }

        // Necessary workaround to prevent column width issues after language change 
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

        private void MainDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is not ViewModel vm)
                return;

            vm.UpdateSelection(MainDataGrid.SelectedItems.OfType<Models.MetalHolding>());
        }

        private void ViewModel_LanguageLayoutRefreshRequested(object? sender, EventArgs e)
        {
            MetalTypeFilterComboBox.Items.Refresh();
            ClassificationFilterComboBox.Items.Refresh();
            RecalculateDataGridColumnWidths();
        }

        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is ViewModel vm)
                vm.LanguageLayoutRefreshRequested -= ViewModel_LanguageLayoutRefreshRequested;

            base.OnClosed(e);
        }
    }
}
