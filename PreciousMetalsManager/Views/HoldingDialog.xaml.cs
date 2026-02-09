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
            DataContext = this; 
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

            if (!decimal.TryParse(PurityTextBox.Text, out var purity) || purity <= 0)
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
    }
}
