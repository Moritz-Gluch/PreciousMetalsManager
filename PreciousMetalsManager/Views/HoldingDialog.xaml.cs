using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using PreciousMetalsManager.Domain;
using PreciousMetalsManager.Models;

namespace PreciousMetalsManager.Views
{
    /// <summary>
    /// Performs basic UI-level input validation.
    /// Business-level validation may be moved to the ViewModel in later iterations.
    /// </summary>
    public partial class HoldingDialog : Window
    {
        private const int MinimumQuantity = 1;
        private const int MinimumPrice = 0;
        private const decimal MinimumWeight = 0.01m;
        private const decimal MinimumPurity = 0.1m;
        private const int PurityDecimalPlaces = 1;
        private const int WeightDecimalPlaces = 2;
        private const string PurityNumberFormat = "F1";
        private const string WeightNumberFormat = "F2";
        private const string PriceNumberFormat = "F2";
        private const decimal MinimumPositiveWeight = 0.01m;

        private static readonly Regex IntegerInputRegex = new(@"^[0-9]+$");
        private static readonly Regex DecimalInputRegex = new(@"^[0-9\.,]+$");

        public CollectableType SelectedCollectableType { get; set; } = CollectableType.Bullion;

        public MetalHolding? NewHolding { get; private set; }
        public bool AddAnotherRequested { get; private set; }

        public bool IsEditMode { get; set; }

        private string _originalFormText = string.Empty;

        private static string DefaultPurityText =>
            DomainReferenceData.PreciousMetals.MaximumFinenessPermille.ToString(PurityNumberFormat, CultureInfo.InvariantCulture);

        private static string DefaultWeightText =>
            1m.ToString(WeightNumberFormat, CultureInfo.InvariantCulture);

        private static string DefaultQuantityText =>
            MinimumQuantity.ToString(CultureInfo.InvariantCulture);

        private static string DefaultPurchasePriceText =>
            0m.ToString(PriceNumberFormat, CultureInfo.InvariantCulture);

        public HoldingDialog()
        {
            InitializeComponent();
            MetalTypeComboBox.ItemsSource = Enum.GetValues(typeof(MetalType));
            MetalTypeComboBox.SelectedIndex = 0;
            PurchaseDatePicker.SelectedDate = DateTime.Today;

            PurityComboBox.Text = DefaultPurityText;
            WeightTextBox.Text = DefaultWeightText;
            QuantityTextBox.Text = DefaultQuantityText;
            PurchasePriceTextBox.Text = DefaultPurchasePriceText;

            Loaded += HoldingDialog_Loaded;
        }

        private static string NormalizeDecimalInput(string text)
            => text.Replace(',', '.');

        private static bool TryParseInvariantDecimal(string text, out decimal value)
            => decimal.TryParse(
                NormalizeDecimalInput(text),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out value);

        private static string FormatPurity(decimal value)
            => value.ToString(PurityNumberFormat, CultureInfo.InvariantCulture);

        private static string FormatWeight(decimal value)
            => value.ToString(WeightNumberFormat, CultureInfo.InvariantCulture);

        private static string FormatPrice(decimal value)
            => value.ToString(PriceNumberFormat, CultureInfo.InvariantCulture);

        private void HoldingDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsEditMode)
            {
                _originalFormText = FormTextBox.Text;
            }
        }

        private bool TryCreateHolding()
        {
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

            if (!TryParseInvariantDecimal(PurityComboBox.Text, out var purity) || purity < MinimumPurity)
            {
                PurityComboBox.Focus();
                return false;
            }

            if (!TryParseInvariantDecimal(WeightTextBox.Text, out var weight) || weight < MinimumWeight)
            {
                WeightTextBox.Focus();
                return false;
            }

            if (!int.TryParse(QuantityTextBox.Text, out var quantity) || quantity < MinimumQuantity)
            {
                QuantityTextBox.Focus();
                return false;
            }

            if (!TryParseInvariantDecimal(PurchasePriceTextBox.Text, out var price) || price < MinimumPrice)
            {
                PurchasePriceTextBox.Focus();
                return false;
            }

            if (PurchaseDatePicker.SelectedDate == null)
            {
                PurchaseDatePicker.Focus();
                return false;
            }

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
            if (int.TryParse(QuantityTextBox.Text, out var value))
            {
                QuantityTextBox.Text = (value + 1).ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                QuantityTextBox.Text = DefaultQuantityText;
            }
        }

        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(QuantityTextBox.Text, out var value) && value > MinimumQuantity)
            {
                QuantityTextBox.Text = (value - 1).ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                QuantityTextBox.Text = DefaultQuantityText;
            }
        }

        private void QuantityTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !IntegerInputRegex.IsMatch(e.Text);
        }

        private void QuantityTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(QuantityTextBox.Text, out var value) || value < MinimumQuantity)
            {
                QuantityTextBox.Text = DefaultQuantityText;
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
            e.Handled = !DecimalInputRegex.IsMatch(e.Text);
        }

        private void PurityComboBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!TryParseInvariantDecimal(PurityComboBox.Text, out var value))
            {
                PurityComboBox.Text = DefaultPurityText;
                return;
            }

            var rounded = Math.Round(value, PurityDecimalPlaces, MidpointRounding.AwayFromZero);

            if (rounded > DomainReferenceData.PreciousMetals.MaximumFinenessPermille)
                rounded = DomainReferenceData.PreciousMetals.MaximumFinenessPermille;
            else if (rounded < DomainReferenceData.PreciousMetals.MinimumFinenessPermille)
                rounded = DomainReferenceData.PreciousMetals.MinimumFinenessPermille;

            PurityComboBox.Text = FormatPurity(rounded);
        }

        private void PurchasePriceTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !DecimalInputRegex.IsMatch(e.Text);
        }

        private void PurchasePriceTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!TryParseInvariantDecimal(PurchasePriceTextBox.Text, out var value) || value < 0)
            {
                PurchasePriceTextBox.Text = DefaultPurchasePriceText;
            }
            else
            {
                PurchasePriceTextBox.Text = FormatPrice(value);
            }
        }

        private void WeightTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !DecimalInputRegex.IsMatch(e.Text);
        }

        private void WeightTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!TryParseInvariantDecimal(WeightTextBox.Text, out var value) || value <= 0)
            {
                WeightTextBox.Text = DefaultWeightText;
                return;
            }

            var rounded = Math.Round(value, WeightDecimalPlaces, MidpointRounding.AwayFromZero);
            if (rounded < MinimumPositiveWeight)
            {
                WeightTextBox.Text = FormatWeight(MinimumPositiveWeight);
                return;
            }

            WeightTextBox.Text = FormatWeight(rounded);
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
