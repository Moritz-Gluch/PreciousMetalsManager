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
    /// Performs basic UI-level input validation.
    /// Business-level validation may be moved to the ViewModel in later iterations.
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

            // Goes sure no field is empty or unvalid
            if (MetalTypeComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a metal type.");
                return;
            }

            if (string.IsNullOrWhiteSpace(FormTextBox.Text))
            {
                MessageBox.Show("Form / Variant is required.");
                return;
            }

            if (!decimal.TryParse(PurityTextBox.Text, out var purity) || purity <= 0)
            {
                MessageBox.Show("Purity must be a positive number.");
                return;
            }

            if (!decimal.TryParse(WeightTextBox.Text, out var weight) || weight <= 0)
            {
                MessageBox.Show("Weight must be a positive number.");
                return;
            }

            if (!int.TryParse(QuantityTextBox.Text, out var quantity) || quantity <= 0)
            {
                MessageBox.Show("Quantity must be a positive whole number.");
                return;
            }

            if (!decimal.TryParse(PurchasePriceTextBox.Text, out var price) || price < 0)
            {
                MessageBox.Show("Purchase price must be zero or greater.");
                return;
            }

            if (PurchaseDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Please select a purchase date.");
                return;
            }

            // Saves new values
            NewHolding = new MetalHolding
            {
                MetalType = (MetalType)MetalTypeComboBox.SelectedItem,
                Form = FormTextBox.Text.Trim(),
                Purity = purity,
                Weight = weight,
                Quantity = quantity,
                PurchasePrice = price,
                PurchaseDate = PurchaseDatePicker.SelectedDate.Value,
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
