using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;
using PreciousMetalsManager.Models;
using PreciousMetalsManager.Services;

namespace PreciousMetalsManager.ViewModels
{
    public sealed class EditPricesDialogViewModel : INotifyPropertyChanged
    {
        private const string PriceNumberFormat = "F2";
        private static readonly string DefaultPriceText = 0m.ToString(PriceNumberFormat, CultureInfo.InvariantCulture);

        private string _goldPriceText;
        private string _silverPriceText;
        private string _platinumPriceText;
        private string _palladiumPriceText;
        private string _broncePriceText;

        public EditPricesDialogViewModel(
            decimal gold,
            decimal silver,
            decimal platinum,
            decimal palladium,
            decimal bronce,
            string priceUnit)
        {
            _goldPriceText = FormatPrice(gold);
            _silverPriceText = FormatPrice(silver);
            _platinumPriceText = FormatPrice(platinum);
            _palladiumPriceText = FormatPrice(palladium);
            _broncePriceText = FormatPrice(bronce);

            PriceUnit = priceUnit;

            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => Cancel());
        }

        public string PriceUnit { get; }

        public string GoldPriceText
        {
            get => _goldPriceText;
            set
            {
                if (_goldPriceText != value)
                {
                    _goldPriceText = value;
                    OnPropertyChanged(nameof(GoldPriceText));
                }
            }
        }

        public string SilverPriceText
        {
            get => _silverPriceText;
            set
            {
                if (_silverPriceText != value)
                {
                    _silverPriceText = value;
                    OnPropertyChanged(nameof(SilverPriceText));
                }
            }
        }

        public string PlatinumPriceText
        {
            get => _platinumPriceText;
            set
            {
                if (_platinumPriceText != value)
                {
                    _platinumPriceText = value;
                    OnPropertyChanged(nameof(PlatinumPriceText));
                }
            }
        }

        public string PalladiumPriceText
        {
            get => _palladiumPriceText;
            set
            {
                if (_palladiumPriceText != value)
                {
                    _palladiumPriceText = value;
                    OnPropertyChanged(nameof(PalladiumPriceText));
                }
            }
        }

        public string BroncePriceText
        {
            get => _broncePriceText;
            set
            {
                if (_broncePriceText != value)
                {
                    _broncePriceText = value;
                    OnPropertyChanged(nameof(BroncePriceText));
                }
            }
        }

        public decimal GoldPrice { get; private set; }
        public decimal SilverPrice { get; private set; }
        public decimal PlatinumPrice { get; private set; }
        public decimal PalladiumPrice { get; private set; }
        public decimal BroncePrice { get; private set; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<bool>? RequestCloseRequested;

        private static string NormalizeDecimalInput(string text)
            => text.Replace(',', '.');

        private static bool TryParseInvariantDecimal(string text, out decimal value)
            => decimal.TryParse(
                NormalizeDecimalInput(text),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out value);

        private static bool TryParseNonNegativePrice(string text, out decimal value)
            => TryParseInvariantDecimal(text, out value) && value >= 0;

        private static string FormatPrice(decimal value)
            => value.ToString(PriceNumberFormat, CultureInfo.InvariantCulture);

        public void NormalizeGoldPrice() => GoldPriceText = NormalizeNonNegativePrice(GoldPriceText);
        public void NormalizeSilverPrice() => SilverPriceText = NormalizeNonNegativePrice(SilverPriceText);
        public void NormalizePlatinumPrice() => PlatinumPriceText = NormalizeNonNegativePrice(PlatinumPriceText);
        public void NormalizePalladiumPrice() => PalladiumPriceText = NormalizeNonNegativePrice(PalladiumPriceText);
        public void NormalizeBroncePrice() => BroncePriceText = NormalizeNonNegativePrice(BroncePriceText);

        private static string NormalizeNonNegativePrice(string text)
        {
            if (!TryParseNonNegativePrice(text, out var value))
                return DefaultPriceText;

            return FormatPrice(value);
        }

        private void Save()
        {
            if (!TryParseNonNegativePrice(GoldPriceText, out var gold))
                return;
            if (!TryParseNonNegativePrice(SilverPriceText, out var silver))
                return;
            if (!TryParseNonNegativePrice(PlatinumPriceText, out var platinum))
                return;
            if (!TryParseNonNegativePrice(PalladiumPriceText, out var palladium))
                return;
            if (!TryParseNonNegativePrice(BroncePriceText, out var bronce))
                return;

            GoldPrice = gold;
            SilverPrice = silver;
            PlatinumPrice = platinum;
            PalladiumPrice = palladium;
            BroncePrice = bronce;

            RequestCloseRequested?.Invoke(this, true);
        }

        private void Cancel()
        {
            RequestCloseRequested?.Invoke(this, false);
        }

        private void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
