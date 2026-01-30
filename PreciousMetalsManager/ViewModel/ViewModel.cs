using System;
using System.Collections.ObjectModel;
using PreciousMetalsManager.Models;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using PreciousMetalsManager.Services;

namespace PreciousMetalsManager.ViewModels
{
    public class ViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<MetalHolding> Holdings { get; }
        public ICollectionView FilteredHoldings { get; }

        private string _formFilter = string.Empty;
        public string FormFilter
        {
            get => _formFilter;
            set
            {
                if (_formFilter != value)
                {
                    _formFilter = value;
                    OnPropertyChanged(nameof(FormFilter));
                    FilteredHoldings.Refresh();
                }
            }
        }

        private object _selectedMetalTypeFilter;
        public object SelectedMetalTypeFilter
        {
            get => _selectedMetalTypeFilter;
            set
            {
                if (_selectedMetalTypeFilter != value)
                {
                    _selectedMetalTypeFilter = value;
                    OnPropertyChanged(nameof(SelectedMetalTypeFilter));
                    FilteredHoldings.Refresh();
                }
            }
        }

        public IEnumerable<object> MetalTypeFilterOptions { get; } =
            new object[] { "All" }.Concat(Enum.GetValues(typeof(MetalType)).Cast<object>());

        private readonly Dictionary<MetalType, decimal> _currentMarketPrices = new();

        public decimal this[MetalType type]
        {
            get => _currentMarketPrices.TryGetValue(type, out var price) ? price : 0m;
            set
            {
                if (_currentMarketPrices.ContainsKey(type) && _currentMarketPrices[type] == value)
                    return;
                _currentMarketPrices[type] = value;
                OnPropertyChanged($"Item[{type}]");
                UpdateCalculatedValues();
            }
        }

        public IEnumerable<MetalType> AllMetalTypes => Enum.GetValues(typeof(MetalType)).Cast<MetalType>();

        private readonly LocalStorageService _storage = new LocalStorageService();

        public ViewModel()
        {
            Holdings = new ObservableCollection<MetalHolding>(_storage.LoadHoldings());
            Holdings.CollectionChanged += Holdings_CollectionChanged;

            FilteredHoldings = CollectionViewSource.GetDefaultView(Holdings);
            FilteredHoldings.Filter = FilterPredicate;

            foreach (var holding in Holdings)
                holding.PropertyChanged += Holding_PropertyChanged;

            UpdateCalculatedValues();
        }

        private void Holdings_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (MetalHolding h in e.NewItems)
                    h.PropertyChanged += Holding_PropertyChanged;
            if (e.OldItems != null)
                foreach (MetalHolding h in e.OldItems)
                    h.PropertyChanged -= Holding_PropertyChanged;

            UpdateCalculatedValues();
        }

        private bool FilterPredicate(object obj)
        {
            if (obj is not MetalHolding holding)
                return false;

            if (SelectedMetalTypeFilter is MetalType type && holding.MetalType != type)
                return false;

            if (!string.IsNullOrWhiteSpace(FormFilter) &&
                !holding.Form.Contains(FormFilter, StringComparison.OrdinalIgnoreCase))
                return false;

            return true;
        }

        private void UpdateCalculatedValues()
        {
            foreach (var holding in Holdings)
            {
                var price = GetMarketPrice(holding.MetalType);
                // A Purity of 999.9 is considered as highest purity (100%) 
                holding.CurrentValue = holding.Weight * (holding.Purity / 999.9m) * price;
                holding.TotalValue = holding.CurrentValue * holding.Quantity;
            }
            OnPropertyChanged(nameof(Holdings));
            if (FilteredHoldings != null)
                FilteredHoldings.Refresh();
        }

        private decimal _goldPrice = 72.50m;
        public decimal GoldPrice
        {
            get => _goldPrice;
            set
            {
                if (_goldPrice != value)
                {
                    _goldPrice = value;
                    OnPropertyChanged(nameof(GoldPrice));
                    UpdateCalculatedValues();
                }
            }
        }

        private decimal _silverPrice = 0.85m;
        public decimal SilverPrice
        {
            get => _silverPrice;
            set
            {
                if (_silverPrice != value)
                {
                    _silverPrice = value;
                    OnPropertyChanged(nameof(SilverPrice));
                    UpdateCalculatedValues();
                }
            }
        }

        private decimal _platinumPrice = 32.10m;
        public decimal PlatinumPrice
        {
            get => _platinumPrice;
            set
            {
                if (_platinumPrice != value)
                {
                    _platinumPrice = value;
                    OnPropertyChanged(nameof(PlatinumPrice));
                    UpdateCalculatedValues();
                }
            }
        }

        private decimal _palladiumPrice = 41.00m;
        public decimal PalladiumPrice
        {
            get => _palladiumPrice;
            set
            {
                if (_palladiumPrice != value)
                {
                    _palladiumPrice = value;
                    OnPropertyChanged(nameof(PalladiumPrice));
                    UpdateCalculatedValues();
                }
            }
        }

        private decimal _broncePrice = 0.10m;
        public decimal BroncePrice
        {
            get => _broncePrice;
            set
            {
                if (_broncePrice != value)
                {
                    _broncePrice = value;
                    OnPropertyChanged(nameof(BroncePrice));
                    UpdateCalculatedValues();
                }
            }
        }

        private decimal GetMarketPrice(MetalType type)
        {
            return type switch
            {
                MetalType.Gold => GoldPrice,
                MetalType.Silver => SilverPrice,
                MetalType.Platinum => PlatinumPrice,
                MetalType.Palladium => PalladiumPrice,
                MetalType.Bronce => BroncePrice,
                _ => 0m
            };
        }

        private void Holding_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MetalHolding.Weight) ||
                e.PropertyName == nameof(MetalHolding.Purity) ||
                e.PropertyName == nameof(MetalHolding.Quantity) ||
                e.PropertyName == nameof(MetalHolding.MetalType))
            {
                UpdateCalculatedValues();
            }
        }

        public void AddHolding(MetalHolding holding)
        {
            _storage.AddHolding(holding);
            ReloadHoldings();
        }

        public void UpdateHolding(MetalHolding holding)
        {
            _storage.UpdateHolding(holding, holding.Id);
            ReloadHoldings();
        }

        public void DeleteHolding(MetalHolding holding)
        {
            _storage.DeleteHolding(holding.Id);
            ReloadHoldings();
        }

        // Reloads data from the database to fix visual bug in UI after adding an entry
        private void ReloadHoldings()
        {
            System.Diagnostics.Debug.WriteLine($"ReloadHoldings() called. Holdings before clear: {Holdings.Count}");

            var view = CollectionViewSource.GetDefaultView(Holdings);
            var oldFilter = view.Filter;
            view.Filter = null;

            Holdings.Clear();
            var loaded = _storage.LoadHoldings();
            System.Diagnostics.Debug.WriteLine($"ReloadHoldings() loaded {loaded.Count} entries from DB.");
            foreach (var h in loaded)
                Holdings.Add(h);

            view.Filter = oldFilter; 
            System.Diagnostics.Debug.WriteLine($"ReloadHoldings() after add: {Holdings.Count}");
        }

        private string _languageButtonText = "EN";
        public string LanguageButtonText
        {
            get => _languageButtonText;
            set
            {
                if (_languageButtonText != value)
                {
                    _languageButtonText = value;
                    OnPropertyChanged(nameof(LanguageButtonText));
                }
            }
        }

        public void ToggleLanguage()
        {
            if (LanguageButtonText == "EN")
            {
                PreciousMetalsManager.Resources.Localization.SetLanguage("de");
                LanguageButtonText = "DE";
            }
            else
            {
                PreciousMetalsManager.Resources.Localization.SetLanguage("en");
                LanguageButtonText = "EN";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}