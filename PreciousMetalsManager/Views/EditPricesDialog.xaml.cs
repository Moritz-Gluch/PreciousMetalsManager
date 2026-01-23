using System;
using System.Windows;

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

        public EditPricesDialog(decimal gold, decimal silver, decimal platinum, decimal palladium, decimal bronce)
        {
            InitializeComponent();
            GoldPriceTextBox.Text = gold.ToString("F2");
            SilverPriceTextBox.Text = silver.ToString("F2");
            PlatinumPriceTextBox.Text = platinum.ToString("F2");
            PalladiumPriceTextBox.Text = palladium.ToString("F2");
            BroncePriceTextBox.Text = bronce.ToString("F2");
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(GoldPriceTextBox.Text, out var gold) || gold < 0 ||
                !decimal.TryParse(SilverPriceTextBox.Text, out var silver) || silver < 0 ||
                !decimal.TryParse(PlatinumPriceTextBox.Text, out var platinum) || platinum < 0 ||
                !decimal.TryParse(PalladiumPriceTextBox.Text, out var palladium) || palladium < 0 ||
                !decimal.TryParse(BroncePriceTextBox.Text, out var bronce) || bronce < 0)
            {
                MessageBox.Show("Bitte geben Sie für alle Metalle einen gültigen, positiven Preis ein.");
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
    }
}
