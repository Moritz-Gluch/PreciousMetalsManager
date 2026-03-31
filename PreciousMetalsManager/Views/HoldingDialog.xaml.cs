using System.Text.RegularExpressions;
using System.Windows;
using PreciousMetalsManager.ViewModels;

namespace PreciousMetalsManager.Views
{
    public partial class HoldingDialog : Window
    {
        private static readonly Regex IntegerInputRegex = new(@"^[0-9]+$");
        private static readonly Regex DecimalInputRegex = new(@"^[0-9\.,]+$");

        public HoldingDialog()
        {
            InitializeComponent();
        }

        public HoldingDialog(HoldingDialogViewModel viewModel)
            : this()
        {
            DataContext = viewModel;

            viewModel.RequestCloseRequested += (_, accepted) =>
            {
                DialogResult = accepted;
                Close();
            };
        }

        public HoldingDialogViewModel? ViewModel => DataContext as HoldingDialogViewModel;

        private void QuantityTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !IntegerInputRegex.IsMatch(e.Text);
        }

        private void PurityComboBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !DecimalInputRegex.IsMatch(e.Text);
        }

        private void PurchasePriceTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !DecimalInputRegex.IsMatch(e.Text);
        }

        private void WeightTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !DecimalInputRegex.IsMatch(e.Text);
        }

        private void QuantityTextBox_LostFocus(object sender, RoutedEventArgs e) => ViewModel?.NormalizeQuantityText();
        private void FormTextBox_LostFocus(object sender, RoutedEventArgs e) => ViewModel?.RestoreFormTextIfNeeded();
        private void PurityComboBox_LostFocus(object sender, RoutedEventArgs e) => ViewModel?.NormalizePurityText();
        private void PurchasePriceTextBox_LostFocus(object sender, RoutedEventArgs e) => ViewModel?.NormalizePurchasePriceText();
        private void WeightTextBox_LostFocus(object sender, RoutedEventArgs e) => ViewModel?.NormalizeWeightText();
        private void PurchaseDatePicker_LostFocus(object sender, RoutedEventArgs e) => ViewModel?.EnsurePurchaseDate();
    }
}