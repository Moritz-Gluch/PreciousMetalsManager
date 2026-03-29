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
        public bool AddAnotherRequested { get; private set; }

        public bool IsEditMode { get; set; }

        private string _originalFormText = string.Empty;

        public HoldingDialog()
        {
            InitializeComponent();
            MetalTypeComboBox.ItemsSource = Enum.GetValues(typeof(MetalType));
            MetalTypeComboBox.SelectedIndex = 0;
            PurchaseDatePicker.SelectedDate = DateTime.Today;

            Loaded += HoldingDialog_Loaded;
        }

        private void HoldingDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsEditMode)
            {
                _originalFormText = FormTextBox.Text;
            }
        }

        private bool TryCreateHolding()
        {
            // Goes sure no field is empty or unvalid
            if (MetalTypeComboBox.SelectedItem == null)
            {
                MetalTypeComboBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(FormTextBox.Text))
            {
                FormTextBox.BorderBrush = System.Windows.Media.Brushes.IndianRed;
                FormTextBox.BorderThickness = new Thickness(2);
                FormTextBox.Focus();
                return false;
            }

            FormTextBox.ClearValue(BorderBrushProperty);
            FormTextBox.ClearValue(BorderThicknessProperty);

            if (!decimal.TryParse(PurityComboBox.Text.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var purity) || purity <= 0)
            {
                PurityComboBox.Focus();
                return false;
            }

            if (!decimal.TryParse(WeightTextBox.Text.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var weight) || weight <= 0)
            {
                WeightTextBox.Focus();
                return false;
            }

            if (!int.TryParse(QuantityTextBox.Text, out var quantity) || quantity <= 0)
            {
                QuantityTextBox.Focus();
                return false;
            }

            if (!decimal.TryParse(PurchasePriceTextBox.Text.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var price) || price < 0)
            {
                PurchasePriceTextBox.Focus();
                return false;
            }

            if (PurchaseDatePicker.SelectedDate == null)
            {
                PurchaseDatePicker.Focus();
                return false;
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

            return true;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            AddAnotherRequested = false;
            if (!TryCreateHolding())
                return;

            DialogResult = true;
            Close();
        }

        private void AddAnotherButton_Click(object sender, RoutedEventArgs e)
        {
            AddAnotherRequested = true;
            if (!TryCreateHolding())
                return;

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            AddAnotherRequested = false;
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

        private void FormTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (IsEditMode && string.IsNullOrWhiteSpace(FormTextBox.Text))
            {
                FormTextBox.Text = _originalFormText;
            }

            if (!string.IsNullOrWhiteSpace(FormTextBox.Text))
            {
                FormTextBox.ClearValue(BorderBrushProperty);
                FormTextBox.ClearValue(BorderThicknessProperty);
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
            if (!decimal.TryParse(text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var value))
            {
                // Corrects invalid values to 999,9
                PurityComboBox.Text = "999.9";
                return;
            }

            // Rounds the value to 1 decimal place
            var rounded = Math.Round(value, 1, MidpointRounding.AwayFromZero);

            if (rounded > 999.9m)
                rounded = 999.9m;
            else if (rounded < 0.1m)
                rounded = 0.1m;

            PurityComboBox.Text = rounded.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);
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
                // Corrects invalid values to 0.00
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

            // Corrects invalid values
            if (!decimal.TryParse(text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var value) || value <= 0)
            {
                WeightTextBox.Text = "1.00";
                return;
            }

            var formatted = value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);

            if (formatted == "0.00")
            {
                WeightTextBox.Text = "0.01";
                return;
            }

            WeightTextBox.Text = formatted;
        }

        private void PurchaseDatePicker_LostFocus(object sender, RoutedEventArgs e)
        {
            if (PurchaseDatePicker.SelectedDate == null)
            {
                PurchaseDatePicker.SelectedDate = DateTime.Today;
            }
        }
    }
}
