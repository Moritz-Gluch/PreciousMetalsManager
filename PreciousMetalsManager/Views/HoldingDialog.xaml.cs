using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using PreciousMetalsManager.Models;

namespace PreciousMetalsManager.Views
{
    /// <summary>
    /// Interaktionslogik für HoldingDialog.xaml
    /// </summary>
    public partial class HoldingDialog : Window
    {
        public MetalHolding NewHolding { get; private set; }

        public HoldingDialog()
        {
            InitializeComponent();

            MetalTypeComboBox.ItemsSource = Enum.GetValues(typeof(MetalType));
            MetalTypeComboBox.SelectedIndex = 0;
            PurchaseDatePicker.SelectedDate = DateTime.Now;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Simple mapping, no validation
            NewHolding = new MetalHolding
            {
                MetalType = (MetalType)MetalTypeComboBox.SelectedItem,
                Form = FormTextBox.Text,
                Purity = decimal.TryParse(PurityTextBox.Text, out var p) ? p : 0,
                Weight = decimal.TryParse(WeightTextBox.Text, out var w) ? w : 0,
                Quantity = int.TryParse(QuantityTextBox.Text, out var q) ? q : 0,
                PurchasePrice = decimal.TryParse(PurchasePriceTextBox.Text, out var pp) ? pp : 0,
                PurchaseDate = PurchaseDatePicker.SelectedDate ?? DateTime.Now,
                CurrentValue = 0,
                TotalValue = 0
            };

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
