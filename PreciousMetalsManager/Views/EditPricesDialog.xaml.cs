using System;
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
        public decimal GoldPrice { get; private set; }
        public decimal SilverPrice { get; private set; }
        public decimal PlatinumPrice { get; private set; }
        public decimal PalladiumPrice { get; private set; }
        public decimal BroncePrice { get; private set; }
        public string PriceUnit { get; }

        private static readonly Regex PriceInputRegex = new(@"^[0-9\.,]+$");

        public EditPricesDialog(decimal gold, decimal silver, decimal platinum, decimal palladium, decimal bronce, string priceUnit)
        {
            InitializeComponent();
            GoldPriceTextBox.Text = gold.ToString("F2", CultureInfo.InvariantCulture);
            SilverPriceTextBox.Text = silver.ToString("F2", CultureInfo.InvariantCulture);
            PlatinumPriceTextBox.Text = platinum.ToString("F2", CultureInfo.InvariantCulture);
            PalladiumPriceTextBox.Text = palladium.ToString("F2", CultureInfo.InvariantCulture);
            BroncePriceTextBox.Text = bronce.ToString("F2", CultureInfo.InvariantCulture);
            PriceUnit = priceUnit;
            DataContext = this;
        }

        private static string L(string key)
            => Application.Current?.TryFindResource(key) as string ?? key;

        private static bool TryParseNonNegativePrice(string text, out decimal value)
            => decimal.TryParse(text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out value) && value >= 0;

        private static string NormalizeNonNegativePrice(string text)
        {
            if (!TryParseNonNegativePrice(text, out var value))
                return "0.00";

            return value.ToString("F2", CultureInfo.InvariantCulture);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!TryParseNonNegativePrice(GoldPriceTextBox.Text, out var gold))
            {
                MessageBox.Show(L("EditPricesDialog_Msg_InvalidPrice"));
                return;
            }

            if (!TryParseNonNegativePrice(SilverPriceTextBox.Text, out var silver))
            {
                MessageBox.Show(L("EditPricesDialog_Msg_InvalidPrice"));
                return;
            }

            if (!TryParseNonNegativePrice(PlatinumPriceTextBox.Text, out var platinum))
            {
                MessageBox.Show(L("EditPricesDialog_Msg_InvalidPrice"));
                return;
            }

            if (!TryParseNonNegativePrice(PalladiumPriceTextBox.Text, out var palladium))
            {
                MessageBox.Show(L("EditPricesDialog_Msg_InvalidPrice"));
                return;
            }

            if (!TryParseNonNegativePrice(BroncePriceTextBox.Text, out var bronce))
            {
                MessageBox.Show(L("EditPricesDialog_Msg_InvalidPrice"));
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
            // Allows only numbers, commas and dots to be entered
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
