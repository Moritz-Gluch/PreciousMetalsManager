using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using PreciousMetalsManager.ViewModels;

namespace PreciousMetalsManager.Views
{
    public partial class EditPricesDialog : Window
    {
        private static readonly Regex PriceInputRegex = new(@"^[0-9\.,]+$");

        public EditPricesDialog(EditPricesDialogViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;

            viewModel.RequestCloseRequested += (_, accepted) =>
            {
                DialogResult = accepted;
                Close();
            };
        }

        public EditPricesDialogViewModel ViewModel => (EditPricesDialogViewModel)DataContext;

        private void PriceTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !PriceInputRegex.IsMatch(e.Text);
        }

        private void GoldPriceTextBox_LostFocus(object sender, RoutedEventArgs e) => ViewModel.NormalizeGoldPrice();
        private void SilverPriceTextBox_LostFocus(object sender, RoutedEventArgs e) => ViewModel.NormalizeSilverPrice();
        private void PlatinumPriceTextBox_LostFocus(object sender, RoutedEventArgs e) => ViewModel.NormalizePlatinumPrice();
        private void PalladiumPriceTextBox_LostFocus(object sender, RoutedEventArgs e) => ViewModel.NormalizePalladiumPrice();
        private void BroncePriceTextBox_LostFocus(object sender, RoutedEventArgs e) => ViewModel.NormalizeBroncePrice();
    }
}
