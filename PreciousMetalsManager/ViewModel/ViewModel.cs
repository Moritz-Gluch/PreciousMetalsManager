using System;
using System.Collections.ObjectModel;
using PreciousMetalsManager.Models;
using System.ComponentModel;
using System.Windows.Data;
using System.Linq;
using System.Collections.Specialized;
using PreciousMetalsManager.Services;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace PreciousMetalsManager.ViewModels
{
    public class ViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<MetalHolding> Holdings { get; }
        public ICollectionView FilteredHoldings { get; }

        private static string L(string key)
            => Application.Current?.TryFindResource(key) as string ?? key;

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

        private object? _selectedMetalTypeFilter;
        public object? SelectedMetalTypeFilter
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
            new object[] { L("Filter_All") }.Concat(Enum.GetValues(typeof(MetalType)).Cast<object>());

        private readonly LocalStorageService _storage = new LocalStorageService();
        private readonly MetalPriceApiService _metalPriceApiService = new MetalPriceApiService();

        private readonly DispatcherTimer _autoRefreshTimer;

        public ViewModel()
        {
            Holdings = new ObservableCollection<MetalHolding>(_storage.LoadHoldings());
            Holdings.CollectionChanged += Holdings_CollectionChanged;

            FilteredHoldings = CollectionViewSource.GetDefaultView(Holdings);
            FilteredHoldings.Filter = FilterPredicate;

            foreach (var holding in Holdings)
                holding.PropertyChanged += Holding_PropertyChanged;

            UpdateCalculatedValues();

            // Fetch current market prices on startup
            _ = UpdateMarketPricesAsync();

            RefreshPricesCommand = new RelayCommand(async _ => await UpdateMarketPricesAsync());

            // Auto-refresh every 15 minutes
            _autoRefreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(15)
            };
            _autoRefreshTimer.Tick += async (s, e) => await UpdateMarketPricesAsync();
            _autoRefreshTimer.Start();
        }

        private void Holdings_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
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
            FilteredHoldings.Refresh();
        }

        private decimal _goldPrice;
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

        private decimal _silverPrice;
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

        private decimal _platinumPrice;
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

        private decimal _palladiumPrice;
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

        private decimal _broncePrice;
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

        private void Holding_PropertyChanged(object? sender, PropertyChangedEventArgs e)
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

        public void ToggleLanguage()
        {
            App.SetLanguage(App.CurrentLanguage == "en" ? "de" : "en");
        }

        public async Task UpdateMarketPricesAsync()
        {
            var dto = await _metalPriceApiService.FetchMetalPricesAsync();
            if (dto == null)
            {
                ShowErrorMessage(L("Msg_PriceApiError"), L("Msg_ErrorTitle"));
                return;
            }

            // 1 troy ounce = 31.1g (may be adjusted to the exact value in the future)
            const decimal gramsPerOunce = 31.1m;
            GoldPrice = Math.Round(dto.GoldEur / gramsPerOunce, 2);
            SilverPrice = Math.Round(dto.SilverEur / gramsPerOunce, 2);
            PlatinumPrice = Math.Round(dto.PlatinumEur / gramsPerOunce, 2);
            PalladiumPrice = Math.Round(dto.PalladiumEur / gramsPerOunce, 2);
            // Bronce price is not avaiable on used api, must currently be added manually  
        }

        public ICommand RefreshPricesCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void ShowErrorMessage(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}