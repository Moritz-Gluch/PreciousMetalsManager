using PreciousMetalsManager.ViewModels;
using PreciousMetalsManager.Views;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new HoldingDialog();
            if (addWindow.ShowDialog() == true)
            {
                if (DataContext is ViewModel vm)
                {
                    vm.Holdings.Add(addWindow.NewHolding);
                }
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModel vm)
            {
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

                    if (editWindow.ShowDialog() == true)
                    {
                        // Adopt changes
                        selected.MetalType = editWindow.NewHolding.MetalType;
                        selected.Form = editWindow.NewHolding.Form;
                        selected.Purity = editWindow.NewHolding.Purity;
                        selected.Weight = editWindow.NewHolding.Weight;
                        selected.Quantity = editWindow.NewHolding.Quantity;
                        selected.PurchasePrice = editWindow.NewHolding.PurchasePrice;
                        selected.PurchaseDate = editWindow.NewHolding.PurchaseDate;
                        selected.CurrentValue = editWindow.NewHolding.CurrentValue;
                        selected.TotalValue = editWindow.NewHolding.TotalValue;

                        MainDataGrid.Items.Refresh();
                    }
                }
                else
                {
                    MessageBox.Show("Please select a holding to edit.");
                }
            }
        }
    }
}
