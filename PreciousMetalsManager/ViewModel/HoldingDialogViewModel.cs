using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using PreciousMetalsManager.Domain;
using PreciousMetalsManager.Models;

namespace PreciousMetalsManager.ViewModels
{
    public sealed class HoldingDialogViewModel : INotifyPropertyChanged
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

        private MetalType _selectedMetalType = MetalType.Gold;
        private string _formText = string.Empty;
        private string _purityText;
        private string _weightText;
        private string _quantityText;
        private string _purchasePriceText;
        private DateTime? _purchaseDate;
        private CollectableType _selectedCollectableType = CollectableType.Bullion;

        public HoldingDialogViewModel()
        {
            _purityText = DefaultPurityText;
            _weightText = DefaultWeightText;
            _quantityText = DefaultQuantityText;
            _purchasePriceText = DefaultPurchasePriceText;
            _purchaseDate = DateTime.Today;

            SaveCommand = new RelayCommand(_ => Save());
            AddAnotherCommand = new RelayCommand(_ => AddAnother(), _ => !IsEditMode);
            CancelCommand = new RelayCommand(_ => Cancel());
            IncreaseQuantityCommand = new RelayCommand(_ => IncreaseQuantity());
            DecreaseQuantityCommand = new RelayCommand(_ => DecreaseQuantity());
        }

        public HoldingDialogViewModel(MetalHolding holding)
            : this()
        {
            IsEditMode = true;
            SelectedMetalType = holding.MetalType;
            FormText = holding.Form;
            OriginalFormText = holding.Form;
            PurityText = holding.Purity.ToString(PurityNumberFormat, CultureInfo.InvariantCulture);
            WeightText = holding.Weight.ToString(WeightNumberFormat, CultureInfo.InvariantCulture);
            QuantityText = holding.Quantity.ToString(CultureInfo.InvariantCulture);
            PurchasePriceText = holding.PurchasePrice.ToString(PriceNumberFormat, CultureInfo.InvariantCulture);
            PurchaseDate = holding.PurchaseDate;
            SelectedCollectableType = holding.CollectableType;
        }

        private static string DefaultPurityText =>
            DomainReferenceData.PreciousMetals.MaximumFinenessPermille.ToString(PurityNumberFormat, CultureInfo.InvariantCulture);

        private static string DefaultWeightText =>
            1m.ToString(WeightNumberFormat, CultureInfo.InvariantCulture);

        private static string DefaultQuantityText =>
            MinimumQuantity.ToString(CultureInfo.InvariantCulture);

        private static string DefaultPurchasePriceText =>
            0m.ToString(PriceNumberFormat, CultureInfo.InvariantCulture);

        public ObservableCollection<string> CommonPurities { get; } = new(
            DomainReferenceData.PreciousMetals.CommonFinenessValues
                .Select(value => value.ToString(PurityNumberFormat, CultureInfo.InvariantCulture)));

        public string CurrencyUnit => DomainReferenceData.Currency.CurrencyUnit;
        public string WeightUnit => DomainReferenceData.Currency.WeightUnit;
        public string PurityUnit => DomainReferenceData.Currency.PurityUnit;

        public bool IsEditMode { get; }
        public string OriginalFormText { get; private set; } = string.Empty;

        public MetalType SelectedMetalType
        {
            get => _selectedMetalType;
            set
            {
                if (_selectedMetalType != value)
                {
                    _selectedMetalType = value;
                    OnPropertyChanged(nameof(SelectedMetalType));
                }
            }
        }

        public string FormText
        {
            get => _formText;
            set
            {
                if (_formText != value)
                {
                    _formText = value;
                    OnPropertyChanged(nameof(FormText));
                }
            }
        }

        public string PurityText
        {
            get => _purityText;
            set
            {
                if (_purityText != value)
                {
                    _purityText = value;
                    OnPropertyChanged(nameof(PurityText));
                }
            }
        }

        public string WeightText
        {
            get => _weightText;
            set
            {
                if (_weightText != value)
                {
                    _weightText = value;
                    OnPropertyChanged(nameof(WeightText));
                }
            }
        }

        public string QuantityText
        {
            get => _quantityText;
            set
            {
                if (_quantityText != value)
                {
                    _quantityText = value;
                    OnPropertyChanged(nameof(QuantityText));
                }
            }
        }

        public string PurchasePriceText
        {
            get => _purchasePriceText;
            set
            {
                if (_purchasePriceText != value)
                {
                    _purchasePriceText = value;
                    OnPropertyChanged(nameof(PurchasePriceText));
                }
            }
        }

        public DateTime? PurchaseDate
        {
            get => _purchaseDate;
            set
            {
                if (_purchaseDate != value)
                {
                    _purchaseDate = value;
                    OnPropertyChanged(nameof(PurchaseDate));
                }
            }
        }

        public CollectableType SelectedCollectableType
        {
            get => _selectedCollectableType;
            set
            {
                if (_selectedCollectableType != value)
                {
                    _selectedCollectableType = value;
                    OnPropertyChanged(nameof(SelectedCollectableType));
                }
            }
        }

        public MetalHolding? CreatedHolding { get; private set; }
        public bool AddAnotherRequested { get; private set; }

        public ICommand SaveCommand { get; }
        public ICommand AddAnotherCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand IncreaseQuantityCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<bool>? RequestCloseRequested;
        public event EventHandler? RequestFormTextFocus;

        private static string NormalizeDecimalInput(string text)
            => text.Replace(',', '.');

        private static bool TryParseInvariantDecimal(string text, out decimal value)
            => decimal.TryParse(
                NormalizeDecimalInput(text),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out value);

        public void RestoreFormTextIfNeeded()
        {
            if (IsEditMode && string.IsNullOrWhiteSpace(FormText))
                FormText = OriginalFormText;
        }

        public void NormalizePurityText()
        {
            if (!TryParseInvariantDecimal(PurityText, out var value))
            {
                PurityText = DefaultPurityText;
                return;
            }

            var rounded = Math.Round(value, PurityDecimalPlaces, MidpointRounding.AwayFromZero);

            if (rounded > DomainReferenceData.PreciousMetals.MaximumFinenessPermille)
                rounded = DomainReferenceData.PreciousMetals.MaximumFinenessPermille;
            else if (rounded < DomainReferenceData.PreciousMetals.MinimumFinenessPermille)
                rounded = DomainReferenceData.PreciousMetals.MinimumFinenessPermille;

            PurityText = rounded.ToString(PurityNumberFormat, CultureInfo.InvariantCulture);
        }

        public void NormalizeWeightText()
        {
            if (!TryParseInvariantDecimal(WeightText, out var value) || value <= 0)
            {
                WeightText = DefaultWeightText;
                return;
            }

            var rounded = Math.Round(value, WeightDecimalPlaces, MidpointRounding.AwayFromZero);
            if (rounded < MinimumPositiveWeight)
            {
                WeightText = MinimumPositiveWeight.ToString(WeightNumberFormat, CultureInfo.InvariantCulture);
                return;
            }

            WeightText = rounded.ToString(WeightNumberFormat, CultureInfo.InvariantCulture);
        }

        public void NormalizeQuantityText()
        {
            if (!int.TryParse(QuantityText, out var value) || value < MinimumQuantity)
                QuantityText = DefaultQuantityText;
        }

        public void NormalizePurchasePriceText()
        {
            if (!TryParseInvariantDecimal(PurchasePriceText, out var value) || value < 0)
            {
                PurchasePriceText = DefaultPurchasePriceText;
                return;
            }

            PurchasePriceText = value.ToString(PriceNumberFormat, CultureInfo.InvariantCulture);
        }

        public void EnsurePurchaseDate()
        {
            if (PurchaseDate == null)
                PurchaseDate = DateTime.Today;
        }

        private void IncreaseQuantity()
        {
            if (int.TryParse(QuantityText, out var value))
                QuantityText = (value + 1).ToString(CultureInfo.InvariantCulture);
            else
                QuantityText = DefaultQuantityText;
        }

        private void DecreaseQuantity()
        {
            if (int.TryParse(QuantityText, out var value) && value > MinimumQuantity)
                QuantityText = (value - 1).ToString(CultureInfo.InvariantCulture);
            else
                QuantityText = DefaultQuantityText;
        }

        private bool ValidateFormText()
        {
            if (!string.IsNullOrWhiteSpace(FormText))
                return true;

            RequestFormTextFocus?.Invoke(this, EventArgs.Empty);
            return false;
        }

        private void Save()
        {
            AddAnotherRequested = false;

            if (!ValidateFormText())
                return;

            if (!TryCreateHolding(out var holding))
                return;

            CreatedHolding = holding;
            RequestCloseRequested?.Invoke(this, true);
        }

        private void AddAnother()
        {
            AddAnotherRequested = true;

            if (!ValidateFormText())
                return;

            if (!TryCreateHolding(out var holding))
                return;

            CreatedHolding = holding;
            RequestCloseRequested?.Invoke(this, true);
        }

        private void Cancel()
        {
            AddAnotherRequested = false;
            CreatedHolding = null;
            RequestCloseRequested?.Invoke(this, false);
        }

        private bool TryCreateHolding(out MetalHolding? holding)
        {
            holding = null;

            if (string.IsNullOrWhiteSpace(FormText))
                return false;

            if (!TryParseInvariantDecimal(PurityText, out var purity) || purity < MinimumPurity)
                return false;

            if (!TryParseInvariantDecimal(WeightText, out var weight) || weight < MinimumWeight)
                return false;

            if (!int.TryParse(QuantityText, out var quantity) || quantity < MinimumQuantity)
                return false;

            if (!TryParseInvariantDecimal(PurchasePriceText, out var price) || price < MinimumPrice)
                return false;

            if (PurchaseDate == null)
                return false;

            holding = new MetalHolding
            {
                MetalType = SelectedMetalType,
                Form = FormText.Trim(),
                Purity = purity,
                Weight = weight,
                Quantity = quantity,
                PurchasePrice = price,
                PurchaseDate = PurchaseDate.Value,
                CollectableType = SelectedCollectableType
            };

            return true;
        }

        private void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
