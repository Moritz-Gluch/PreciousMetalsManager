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

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
