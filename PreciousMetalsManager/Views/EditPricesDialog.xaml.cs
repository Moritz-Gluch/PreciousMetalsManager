using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PreciousMetalsManager.Views
{
    /// <summary>
    /// Interaktionslogik für EditPricesDialog.xaml
    /// </summary>
    public partial class EditPricesDialog : Window
    {
        private const string PriceNumberFormat = "F2";
        private static readonly string DefaultPriceText = 0m.ToString(PriceNumberFormat, CultureInfo.InvariantCulture);
        private static readonly Regex PriceInputRegex = new(@"^[0-9\.,]+$");

        public decimal GoldPrice { get; private set; }
        public decimal SilverPrice { get; private set; }
        public decimal PlatinumPrice { get; private set; }
        public decimal PalladiumPrice { get; private set; }
        public decimal BroncePrice { get; private set; }
        public string PriceUnit { get; }

        public EditPricesDialog(decimal gold, decimal silver, decimal platinum, decimal palladium, decimal bronce, string priceUnit)
        {
            InitializeComponent();
            GoldPriceTextBox.Text = FormatPrice(gold);
            SilverPriceTextBox.Text = FormatPrice(silver);
            PlatinumPriceTextBox.Text = FormatPrice(platinum);
            PalladiumPriceTextBox.Text = FormatPrice(palladium);
            BroncePriceTextBox.Text = FormatPrice(bronce);
            PriceUnit = priceUnit;
            DataContext = this;
        }

        private static string NormalizeDecimalInput(string text)
            => text.Replace(',', '.');

        private static bool TryParseInvariantDecimal(string text, out decimal value)
            => decimal.TryParse(
                NormalizeDecimalInput(text),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out value);

        private static string FormatPrice(decimal value)
            => value.ToString(PriceNumberFormat, CultureInfo.InvariantCulture);

        private static bool TryParseNonNegativePrice(string text, out decimal value)
            => TryParseInvariantDecimal(text, out value) && value >= 0;

        private static string NormalizeNonNegativePrice(string text)
        {
            if (!TryParseNonNegativePrice(text, out var value))
                return DefaultPriceText;

            return FormatPrice(value);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!TryParseNonNegativePrice(GoldPriceTextBox.Text, out var gold))
            {
                GoldPriceTextBox.Focus();
                return;
            }

            if (!TryParseNonNegativePrice(SilverPriceTextBox.Text, out var silver))
            {
                SilverPriceTextBox.Focus();
                return;
            }

            if (!TryParseNonNegativePrice(PlatinumPriceTextBox.Text, out var platinum))
            {
                PlatinumPriceTextBox.Focus();
                return;
            }

            if (!TryParseNonNegativePrice(PalladiumPriceTextBox.Text, out var palladium))
            {
                PalladiumPriceTextBox.Focus();
                return;
            }

            if (!TryParseNonNegativePrice(BroncePriceTextBox.Text, out var bronce))
            {
                BroncePriceTextBox.Focus();
                return;
            }

            GoldPrice = gold;
            SilverPrice = silver;
            PlatinumPrice = platinum;
            PalladiumPrice = palladium;
            BroncePrice = bronce;

            DialogResult = true;
            Close();
        }

        private void PriceTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !PriceInputRegex.IsMatch(e.Text);
        }

        private void PriceTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is not TextBox tb)
                return;

            tb.Text = NormalizeNonNegativePrice(tb.Text);
        }
    }
}
