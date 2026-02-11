using System;
using System.ComponentModel;

namespace PreciousMetalsManager.Models
{
    public enum MetalType
    {
        Gold,
        Silver,
        Bronce,
        Platinum,
        Palladium
    }

    public enum CollectableType
    {
        Bullion = 0,
        SemiNumismatic = 1,
        Numismatic = 2
    }

    public class MetalHolding : INotifyPropertyChanged
    {
        public int Id { get; set; }

        private MetalType _metalType;
        public MetalType MetalType
        {
            get => _metalType;
            set { if (_metalType != value) { _metalType = value; OnPropertyChanged(nameof(MetalType)); } }
        }

        private string _form = string.Empty;
        public string Form
        {
            get => _form;
            set { if (_form != value) { _form = value; OnPropertyChanged(nameof(Form)); } }
        }

        private decimal _purity;
        public decimal Purity
        {
            get => _purity;
            set { if (_purity != value) { _purity = value; OnPropertyChanged(nameof(Purity)); } }
        }

        private decimal _weight;
        public decimal Weight
        {
            get => _weight;
            set { if (_weight != value) { _weight = value; OnPropertyChanged(nameof(Weight)); } }
        }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set { if (_quantity != value) { _quantity = value; OnPropertyChanged(nameof(Quantity)); } }
        }

        private decimal _purchasePrice;
        public decimal PurchasePrice
        {
            get => _purchasePrice;
            set { if (_purchasePrice != value) { _purchasePrice = value; OnPropertyChanged(nameof(PurchasePrice)); } }
        }

        private DateTime _purchaseDate;
        public DateTime PurchaseDate
        {
            get => _purchaseDate;
            set { if (_purchaseDate != value) { _purchaseDate = value; OnPropertyChanged(nameof(PurchaseDate)); } }
        }

        private decimal _currentValue;
        public decimal CurrentValue
        {
            get => _currentValue;
            set { if (_currentValue != value) { _currentValue = value; OnPropertyChanged(nameof(CurrentValue)); } }
        }

        private decimal _totalValue;
        public decimal TotalValue
        {
            get => _totalValue;
            set { if (_totalValue != value) { _totalValue = value; OnPropertyChanged(nameof(TotalValue)); } }
        }

        private CollectableType _collectableType;
        public CollectableType CollectableType
        {
            get => _collectableType;
            set { if (_collectableType != value) { _collectableType = value; OnPropertyChanged(nameof(CollectableType)); } }
        }

        private static string L(string key)
            => System.Windows.Application.Current?.TryFindResource(key) as string ?? key;

        /// <summary>
        /// Returns "Yes" if the holding period is at least 1 year, otherwise "X days"
        /// </summary>
        public string TaxFreeStatus
        {
            get
            {
                if (PurchaseDate == default)
                    return string.Empty;

                var taxFreeDate = PurchaseDate.Date.AddYears(1);
                if (DateTime.Today >= taxFreeDate)
                    return L("TaxFreeStatus_Yes");
                else
                    return $"{(taxFreeDate - DateTime.Today).Days} {L("TaxFreeStatus_DaysLeft")}";
            }
        }

        /// <summary>
        /// True if the holding period is at least 1 year.
        /// </summary>
        public bool IsTaxFree =>
            PurchaseDate != default && DateTime.Today >= PurchaseDate.Date.AddYears(1);

        /// <summary>
        /// Notifies the UI that TaxFreeStatus and IsTaxFree have changed.
        /// </summary>
        public void NotifyTaxFreeStatusChanged()
        {
            OnPropertyChanged(nameof(TaxFreeStatus));
            OnPropertyChanged(nameof(IsTaxFree));
        }

        /// <summary>
        /// Returns 0 if tax-free, otherwise the number of days left.
        /// </summary>
        public int TaxFreeSortValue
        {
            get
            {
                if (PurchaseDate == default)
                    return int.MaxValue; // Unset dates last
                var taxFreeDate = PurchaseDate.Date.AddYears(1);
                return DateTime.Today >= taxFreeDate ? 0 : (taxFreeDate - DateTime.Today).Days;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
