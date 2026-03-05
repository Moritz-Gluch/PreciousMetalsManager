using System;
using System.Windows;
using System.Windows.Controls;
using PreciousMetalsManager.Models;

namespace PreciousMetalsManager.Views
{
    /// <summary>
    /// Performs basic UI-level input validation.
    /// Business-level validation may be moved to the ViewModel in later iterations.
    /// </summary>
    public partial class HoldingDialog : Window
    {
        public CollectableType SelectedCollectableType { get; set; } = CollectableType.Bullion;

        public MetalHolding? NewHolding { get; private set; }

        public HoldingDialog()
        {
            InitializeComponent();
            MetalTypeComboBox.ItemsSource = Enum.GetValues(typeof(MetalType));
            MetalTypeComboBox.SelectedIndex = 0;
            PurchaseDatePicker.SelectedDate = DateTime.Now;
        }

        private static string L(string key)
            => Application.Current?.TryFindResource(key) as string ?? key;

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Goes sure no field is empty or unvalid
            if (MetalTypeComboBox.SelectedItem == null)
            {
                MessageBox.Show(L("HoldingDialog_Msg_SelectMetalType"));
                return;
            }

            if (string.IsNullOrWhiteSpace(FormTextBox.Text))
            {
                MessageBox.Show(L("HoldingDialog_Msg_FormRequired"));
                return;
            }

            if (!decimal.TryParse(PurityComboBox.Text.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var purity) || purity <= 0)
            {
                MessageBox.Show(L("HoldingDialog_Msg_PurityPositive"));
                return;
            }

            if (!decimal.TryParse(WeightTextBox.Text, out var weight) || weight <= 0)
            {
                MessageBox.Show(L("HoldingDialog_Msg_WeightPositive"));
                return;
            }

            if (!int.TryParse(QuantityTextBox.Text, out var quantity) || quantity <= 0)
            {
                MessageBox.Show(L("HoldingDialog_Msg_QuantityPositiveWhole"));
                return;
            }

            if (!decimal.TryParse(PurchasePriceTextBox.Text, out var price) || price < 0)
            {
                MessageBox.Show(L("HoldingDialog_Msg_PurchasePriceNonNegative"));
                return;
            }

            if (PurchaseDatePicker.SelectedDate == null)
            {
                MessageBox.Show(L("HoldingDialog_Msg_SelectPurchaseDate"));
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
                CollectableType = SelectedCollectableType,
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

        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(QuantityTextBox.Text, out int value))
            {
                QuantityTextBox.Text = (value + 1).ToString();
            }
            else
            {
                QuantityTextBox.Text = "1";
            }
        }

        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(QuantityTextBox.Text, out int value) && value > 1)
            {
                QuantityTextBox.Text = (value - 1).ToString();
            }
            else
            {
                QuantityTextBox.Text = "1";
            }
        }

        private void QuantityTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Allows only numbers to be entered
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"^[0-9]+$");
        }

        private void QuantityTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Corrects invalid values to 1
            if (!int.TryParse(QuantityTextBox.Text, out int value) || value < 1)
            {
                QuantityTextBox.Text = "1";
            }
        }

        private void PurityComboBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Allows only numbers, commas and dots to be entered
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"^[0-9\.,]+$");
        }

        private void PurityComboBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var text = PurityComboBox.Text.Replace(',', '.');
            if (!decimal.TryParse(text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var value) || value <= 0 || value >= 1000)
            {
                // Corrects invalid values to 999,9
                PurityComboBox.Text = "999,9";
            }
        }

        private void PurchasePriceTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Allows only numbers, commas and dots to be entered
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"^[0-9\.,]+$");
        }

        private void PurchasePriceTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var text = PurchasePriceTextBox.Text.Replace(',', '.');
            if (!decimal.TryParse(text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var value) || value < 0)
            {
                // Corrects invalid values to 0
                PurchasePriceTextBox.Text = "0.00";
            }
            else
            {
                // Formats the value to 2 decimal places
                PurchasePriceTextBox.Text = value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        private void WeightTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Allows only numbers, commas and dots to be entered
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"^[0-9\.,]+$");
        }

        private void WeightTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var text = WeightTextBox.Text.Replace(',', '.');
            if (!decimal.TryParse(text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var value) || value <= 0)
            {
                // Corrects invalid values to 1.00
                WeightTextBox.Text = "1.00";
            }
            else
            {
                // Formats the value to 2 decimal places
                WeightTextBox.Text = value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
            }
        }
    }
}
