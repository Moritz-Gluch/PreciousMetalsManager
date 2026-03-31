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
using Microsoft.Win32;
using System.Globalization;
using PreciousMetalsManager.Domain;
using PreciousMetalsManager.Views;
using System.Collections.Generic;

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
                    RefreshFilteredView();
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
                    RefreshFilteredView();
                }
            }
        }

        private ObservableCollection<object> _metalTypeFilterOptions = new ObservableCollection<object>();
        public ObservableCollection<object> MetalTypeFilterOptions
        {
            get => _metalTypeFilterOptions;
            private set
            {
                if (_metalTypeFilterOptions != value)
                {
                    _metalTypeFilterOptions = value;
                    OnPropertyChanged(nameof(MetalTypeFilterOptions));
                }
            }
        }

        private object? _selectedCollectableTypeFilter;
        public object? SelectedCollectableTypeFilter
        {
            get => _selectedCollectableTypeFilter;
            set
            {
                if (_selectedCollectableTypeFilter != value)
                {
                    _selectedCollectableTypeFilter = value;
                    OnPropertyChanged(nameof(SelectedCollectableTypeFilter));
                    RefreshFilteredView();
                }
            }
        }

        private ObservableCollection<object> _collectableTypeFilterOptions = new ObservableCollection<object>();
        public ObservableCollection<object> CollectableTypeFilterOptions
        {
            get => _collectableTypeFilterOptions;
            private set
            {
                if (_collectableTypeFilterOptions != value)
                {
                    _collectableTypeFilterOptions = value;
                    OnPropertyChanged(nameof(CollectableTypeFilterOptions));
                }
            }
        }

        private readonly LocalStorageService _storage = new LocalStorageService();
        private readonly MetalPriceApiService _metalPriceApiService = new MetalPriceApiService();

        private readonly DispatcherTimer _autoRefreshTimer;
        private bool _isReloadingHoldings;

        // Auto-refresh every 15 minutes
        private const int AutoRefreshIntervalMinutes = 15;
        private const int PriceDecimalPlaces = 2;
        private const string PriceNumberFormat = "F2";
        private const string PurityNumberFormat = "F1";
        private const string ExportFileDateFormat = "dd-MM-yyyy";
        private const string ImportExportCsvDateFormat = "yyyy-MM-dd";

        private static decimal ConvertOuncePriceToGramPrice(decimal ouncePrice)
            => Math.Round(
                ouncePrice / DomainReferenceData.PreciousMetals.RoundedTroyOunceInGrams,
                PriceDecimalPlaces);

        private static string FormatPrice(decimal value)
            => value.ToString(PriceNumberFormat, CultureInfo.InvariantCulture);

        public ViewModel(LocalStorageService? storage = null)
        {
            _storage = storage ?? new LocalStorageService();
            Holdings = new ObservableCollection<MetalHolding>(_storage.LoadHoldings());
            Holdings.CollectionChanged += Holdings_CollectionChanged;

            FilteredHoldings = CollectionViewSource.GetDefaultView(Holdings);
            FilteredHoldings.Filter = FilterPredicate;

            foreach (var holding in Holdings)
                holding.PropertyChanged += Holding_PropertyChanged;

            RefreshPricesCommand = new RelayCommand(async _ => await UpdateMarketPricesAsync());

            _autoRefreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(AutoRefreshIntervalMinutes)
            };
            _autoRefreshTimer.Tick += async (s, e) => await UpdateMarketPricesAsync();
            _autoRefreshTimer.Start();

            UpdateFilterOptions(resetSelection: true);
            RecalculateAndRefreshView();

            // Fetch current market prices on startup
            _ = UpdateMarketPricesAsync();

            ExportSimpleCommand = new RelayCommand(_ => ExportSimpleHoldings());
            ExportDetailedCommand = new RelayCommand(_ => ExportDetailedHoldings());
            ImportCommand = new RelayCommand(async _ => await ImportSimpleHoldingsAsync());

            AddHoldingCommand = new RelayCommand(ExecuteAddHolding);
            EditHoldingCommand = new RelayCommand(ExecuteEditHolding, _ => SelectedHolding is not null);
            DeleteSelectedHoldingsCommand = new RelayCommand(ExecuteDeleteSelectedHoldings, _ => HasSelection);
            EditPricesCommand = new RelayCommand(ExecuteEditPrices);
            ToggleLanguageCommand = new RelayCommand(_ =>
            {
                ToggleLanguage();
                LanguageLayoutRefreshRequested?.Invoke(this, EventArgs.Empty);
            });
        }

        private void Holdings_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (MetalHolding h in e.NewItems)
                    h.PropertyChanged += Holding_PropertyChanged;

            if (e.OldItems != null)
                foreach (MetalHolding h in e.OldItems)
                    h.PropertyChanged -= Holding_PropertyChanged;

            if (_isReloadingHoldings)
                return;

            UpdateFilterOptions(resetSelection: false);
            RecalculateAndRefreshView();
        }

        private bool FilterPredicate(object obj)
        {
            if (obj is not MetalHolding holding)
                return false;

            if (SelectedMetalTypeFilter is MetalType type && holding.MetalType != type)
                return false;

            if (SelectedCollectableTypeFilter is CollectableType collectableType &&
                holding.CollectableType != collectableType)
                return false;

            if (!string.IsNullOrWhiteSpace(FormFilter) &&
                !holding.Form.Contains(FormFilter, StringComparison.OrdinalIgnoreCase))
                return false;

            if (TaxFreeOnly && !holding.IsTaxFree)
                return false;

            return true;
        }

        private void RecalculateHoldingValues()
        {
            foreach (var holding in Holdings)
            {
                var price = GetMarketPrice(holding.MetalType);
                holding.CurrentValue =
                    holding.Weight *
                    (holding.Purity / DomainReferenceData.PreciousMetals.MaximumFinenessPermille) *
                    price;
                holding.TotalValue = holding.CurrentValue * holding.Quantity;
            }
        }

        private void RefreshFilteredView()
        {
            FilteredHoldings.Refresh();
            UpdateVisibleHoldingsTotalValue();
        }

        private void RecalculateAndRefreshView()
        {
            RecalculateHoldingValues();
            RefreshFilteredView();
        }

        private bool SetPriceCore(ref decimal field, decimal value, string propertyName, string displayPropertyName)
        {
            if (field == value)
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            OnPropertyChanged(displayPropertyName);
            return true;
        }

        private decimal _goldPrice;
        public decimal GoldPrice
        {
            get => _goldPrice;
            set
            {
                if (SetPriceCore(ref _goldPrice, value, nameof(GoldPrice), nameof(GoldPriceDisplay)))
                    RecalculateAndRefreshView();
            }
        }

        private decimal _silverPrice;
        public decimal SilverPrice
        {
            get => _silverPrice;
            set
            {
                if (SetPriceCore(ref _silverPrice, value, nameof(SilverPrice), nameof(SilverPriceDisplay)))
                    RecalculateAndRefreshView();
            }
        }

        private decimal _platinumPrice;
        public decimal PlatinumPrice
        {
            get => _platinumPrice;
            set
            {
                if (SetPriceCore(ref _platinumPrice, value, nameof(PlatinumPrice), nameof(PlatinumPriceDisplay)))
                    RecalculateAndRefreshView();
            }
        }

        private decimal _palladiumPrice;
        public decimal PalladiumPrice
        {
            get => _palladiumPrice;
            set
            {
                if (SetPriceCore(ref _palladiumPrice, value, nameof(PalladiumPrice), nameof(PalladiumPriceDisplay)))
                    RecalculateAndRefreshView();
            }
        }

        private decimal _broncePrice;
        public decimal BroncePrice
        {
            get => _broncePrice;
            set
            {
                if (SetPriceCore(ref _broncePrice, value, nameof(BroncePrice), nameof(BroncePriceDisplay)))
                    RecalculateAndRefreshView();
            }
        }

        private string _priceUnit = DomainReferenceData.Currency.PricePerGramUnit;
        public string PriceUnit
        {
            get => _priceUnit;
            set
            {
                if (_priceUnit != value)
                {
                    _priceUnit = value;
                    OnPropertyChanged(nameof(PriceUnit));
                    OnPropertyChanged(nameof(GoldPriceDisplay));
                    OnPropertyChanged(nameof(SilverPriceDisplay));
                    OnPropertyChanged(nameof(PlatinumPriceDisplay));
                    OnPropertyChanged(nameof(PalladiumPriceDisplay));
                    OnPropertyChanged(nameof(BroncePriceDisplay));
                }
            }
        }

        public string CurrencyUnit => DomainReferenceData.Currency.CurrencyUnit;
        public string CurrencyUnitSimplyfied => DomainReferenceData.Currency.SimplifiedCurrencyUnit;
        public string WeightUnit => DomainReferenceData.Currency.WeightUnit;
        public string PurityUnit => DomainReferenceData.Currency.PurityUnit;

        public ObservableCollection<string> CommonPurities { get; } = new(
            DomainReferenceData.PreciousMetals.CommonFinenessValues
                .Select(value => value.ToString(PurityNumberFormat, CultureInfo.InvariantCulture)));

        public string GoldPriceDisplay => $"{FormatPrice(GoldPrice)}{PriceUnit}";
        public string SilverPriceDisplay => $"{FormatPrice(SilverPrice)}{PriceUnit}";
        public string PlatinumPriceDisplay => $"{FormatPrice(PlatinumPrice)}{PriceUnit}";
        public string PalladiumPriceDisplay => $"{FormatPrice(PalladiumPrice)}{PriceUnit}";
        public string BroncePriceDisplay => $"{FormatPrice(BroncePrice)}{PriceUnit}";

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
                RecalculateAndRefreshView();
                return;
            }

            if (e.PropertyName == nameof(MetalHolding.Form) ||
                e.PropertyName == nameof(MetalHolding.CollectableType))
            {
                RefreshFilteredView();
                return;
            }

            // Forward TaxFreeStatus and IsTaxFree property changes when PurchaseDate changes
            if (e.PropertyName == nameof(MetalHolding.PurchaseDate) &&
                sender is MetalHolding holding)
            {
                holding.NotifyTaxFreeStatusChanged();
                RefreshFilteredView();
            }
        }

        private bool _taxFreeOnly;
        public bool TaxFreeOnly
        {
            get => _taxFreeOnly;
            set
            {
                if (_taxFreeOnly != value)
                {
                    _taxFreeOnly = value;
                    OnPropertyChanged(nameof(TaxFreeOnly));
                    RefreshFilteredView();
                }
            }
        }

        private decimal _visibleHoldingsTotalValue;
        public decimal VisibleHoldingsTotalValue
        {
            get => _visibleHoldingsTotalValue;
            private set
            {
                if (_visibleHoldingsTotalValue != value)
                {
                    _visibleHoldingsTotalValue = value;
                    OnPropertyChanged(nameof(VisibleHoldingsTotalValue));
                }
            }
        }

        // Calculates total value of visible holdings
        private void UpdateVisibleHoldingsTotalValue()
        {
            decimal total = 0m;

            foreach (var item in FilteredHoldings)
            {
                if (item is MetalHolding holding)
                    total += holding.TotalValue;
            }

            VisibleHoldingsTotalValue = total;
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

            _isReloadingHoldings = true;

            try
            {
                view.Filter = null;

                Holdings.Clear();
                var loaded = _storage.LoadHoldings();
                System.Diagnostics.Debug.WriteLine($"ReloadHoldings() loaded {loaded.Count} entries from DB.");

                foreach (var h in loaded)
                    Holdings.Add(h);
            }
            finally
            {
                _isReloadingHoldings = false;
                view.Filter = oldFilter;
            }

            UpdateFilterOptions(resetSelection: true);
            RecalculateAndRefreshView();

            System.Diagnostics.Debug.WriteLine($"ReloadHoldings() after add: {Holdings.Count}");
        }

        public void ToggleLanguage()
        {
            App.SetLanguage(App.CurrentLanguage == "en" ? "de" : "en");
            UpdateFilterOptions(resetSelection: false);
            RefreshFilteredView();
        }

        public async Task UpdateMarketPricesAsync()
        {
            var dto = await _metalPriceApiService.FetchMetalPricesAsync();
            if (dto == null)
            {
                ShowErrorMessage(L("Msg_PriceApiError"), L("Msg_ErrorTitle"));
                return;
            }

            var goldPrice = ConvertOuncePriceToGramPrice(dto.GoldEur);
            var silverPrice = ConvertOuncePriceToGramPrice(dto.SilverEur);
            var platinumPrice = ConvertOuncePriceToGramPrice(dto.PlatinumEur);
            var palladiumPrice = ConvertOuncePriceToGramPrice(dto.PalladiumEur);

            var hasChanges = false;
            hasChanges |= SetPriceCore(ref _goldPrice, goldPrice, nameof(GoldPrice), nameof(GoldPriceDisplay));
            hasChanges |= SetPriceCore(ref _silverPrice, silverPrice, nameof(SilverPrice), nameof(SilverPriceDisplay));
            hasChanges |= SetPriceCore(ref _platinumPrice, platinumPrice, nameof(PlatinumPrice), nameof(PlatinumPriceDisplay));
            hasChanges |= SetPriceCore(ref _palladiumPrice, palladiumPrice, nameof(PalladiumPrice), nameof(PalladiumPriceDisplay));

            if (hasChanges)
                RecalculateAndRefreshView();

            // Bronce price is not available on used api, must currently be added manually
        }

        private void UpdateMetalTypeFilterOptions(bool resetSelection)
        {
            var typesInHoldings = Holdings
                .Select(h => h.MetalType)
                .Distinct()
                .Cast<object>()
                .ToList();

            var allOption = L("Filter_All");
            typesInHoldings.Insert(0, allOption);

            MetalTypeFilterOptions = new ObservableCollection<object>(typesInHoldings);

            object? newSelection = _selectedMetalTypeFilter;

            if (resetSelection)
            {
                newSelection = allOption;
            }
            else if (_selectedMetalTypeFilter is string)
            {
                newSelection = allOption;
            }
            else if (_selectedMetalTypeFilter is MetalType selectedType &&
                     !typesInHoldings.Contains(selectedType))
            {
                newSelection = allOption;
            }

            if (!Equals(_selectedMetalTypeFilter, newSelection))
            {
                _selectedMetalTypeFilter = newSelection;
                OnPropertyChanged(nameof(SelectedMetalTypeFilter));
            }
        }

        private void UpdateCollectableTypeFilterOptions(bool resetSelection)
        {
            var typesInHoldings = Holdings
                .Select(h => h.CollectableType)
                .Distinct()
                .Cast<object>()
                .ToList();

            var allOption = L("Filter_All");
            typesInHoldings.Insert(0, allOption);

            CollectableTypeFilterOptions = new ObservableCollection<object>(typesInHoldings);

            object? newSelection = _selectedCollectableTypeFilter;

            if (resetSelection)
            {
                newSelection = allOption;
            }
            else if (_selectedCollectableTypeFilter is string)
            {
                newSelection = allOption;
            }
            else if (_selectedCollectableTypeFilter is CollectableType selectedType &&
                     !typesInHoldings.Contains(selectedType))
            {
                newSelection = allOption;
            }

            if (!Equals(_selectedCollectableTypeFilter, newSelection))
            {
                _selectedCollectableTypeFilter = newSelection;
                OnPropertyChanged(nameof(SelectedCollectableTypeFilter));
            }
        }

        private void UpdateFilterOptions(bool resetSelection)
        {
            UpdateMetalTypeFilterOptions(resetSelection);
            UpdateCollectableTypeFilterOptions(resetSelection);
        }

        private MetalHolding? _selectedHolding;
        public MetalHolding? SelectedHolding
        {
            get => _selectedHolding;
            set
            {
                if (!ReferenceEquals(_selectedHolding, value))
                {
                    _selectedHolding = value;
                    OnPropertyChanged(nameof(SelectedHolding));
                    OnPropertyChanged(nameof(HasSingleSelection));
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        private IReadOnlyList<MetalHolding> _selectedHoldings = Array.Empty<MetalHolding>();
        public IReadOnlyList<MetalHolding> SelectedHoldings
        {
            get => _selectedHoldings;
            private set
            {
                if (!ReferenceEquals(_selectedHoldings, value))
                {
                    _selectedHoldings = value;
                    OnPropertyChanged(nameof(SelectedHoldings));
                    OnPropertyChanged(nameof(HasSelection));
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public bool HasSelection => SelectedHoldings.Count > 0;
        public bool HasSingleSelection => SelectedHolding is not null;

        public event EventHandler? LanguageLayoutRefreshRequested;

        public void UpdateSelection(IEnumerable<MetalHolding> selectedHoldings)
        {
            var snapshot = selectedHoldings.ToList();
            SelectedHoldings = snapshot;

            if (snapshot.Count == 1)
            {
                SelectedHolding = snapshot[0];
            }
            else if (SelectedHolding is not null && !snapshot.Contains(SelectedHolding))
            {
                SelectedHolding = snapshot.FirstOrDefault();
            }
            else if (snapshot.Count == 0)
            {
                SelectedHolding = null;
            }
        }

        private static Window? GetOwnerWindow(object? parameter)
            => parameter as Window;

        private void ExecuteAddHolding(object? parameter)
        {
            var owner = GetOwnerWindow(parameter);

            var keepAdding = true;
            while (keepAdding)
            {
                var addWindow = new HoldingDialog
                {
                    DataContext = this,
                    Owner = owner
                };

                if (addWindow.ShowDialog() == true && addWindow.NewHolding is { } newHolding)
                {
                    AddHolding(newHolding);
                    keepAdding = addWindow.AddAnotherRequested;
                }
                else
                {
                    keepAdding = false;
                }
            }
        }

        private void ExecuteEditHolding(object? parameter)
        {
            if (SelectedHolding is not MetalHolding selected)
            {
                MessageBox.Show(L("Msg_SelectHoldingToEdit"));
                return;
            }

            var editWindow = new HoldingDialog
            {
                DataContext = this,
                Owner = GetOwnerWindow(parameter),
                IsEditMode = true
            };

            editWindow.MetalTypeComboBox.SelectedItem = selected.MetalType;
            editWindow.FormTextBox.Text = selected.Form;
            editWindow.PurityComboBox.Text = selected.Purity.ToString();
            editWindow.WeightTextBox.Text = selected.Weight.ToString();
            editWindow.QuantityTextBox.Text = selected.Quantity.ToString();
            editWindow.PurchasePriceTextBox.Text = selected.PurchasePrice.ToString();
            editWindow.PurchaseDatePicker.SelectedDate = selected.PurchaseDate;
            editWindow.SelectedCollectableType = selected.CollectableType;

            if (editWindow.ShowDialog() == true && editWindow.NewHolding is { } edited)
            {
                selected.MetalType = edited.MetalType;
                selected.Form = edited.Form;
                selected.Purity = edited.Purity;
                selected.Weight = edited.Weight;
                selected.Quantity = edited.Quantity;
                selected.PurchasePrice = edited.PurchasePrice;
                selected.PurchaseDate = edited.PurchaseDate;
                selected.CollectableType = edited.CollectableType;

                UpdateHolding(selected);
            }
        }

        private void ExecuteDeleteSelectedHoldings(object? parameter)
        {
            var selectedItems = SelectedHoldings.ToList();

            if (selectedItems.Count == 0)
            {
                MessageBox.Show(L("Msg_SelectHoldingToDelete"));
                return;
            }

            MessageBoxResult result;
            if (selectedItems.Count == 1)
            {
                result = MessageBox.Show(
                    L("Msg_ConfirmDeleteText"),
                    L("Msg_ConfirmDeleteTitle"),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
            }
            else
            {
                result = MessageBox.Show(
                    string.Format(L("Msg_ConfirmDeleteMultipleText"), selectedItems.Count),
                    L("Msg_ConfirmDeleteTitle"),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
            }

            if (result == MessageBoxResult.Yes)
            {
                foreach (var holding in selectedItems)
                    DeleteHolding(holding);

                UpdateSelection(Array.Empty<MetalHolding>());
            }
        }

        private void ExecuteEditPrices(object? parameter)
        {
            var dlg = new EditPricesDialog(
                GoldPrice,
                SilverPrice,
                PlatinumPrice,
                PalladiumPrice,
                BroncePrice,
                PriceUnit)
            {
                Owner = GetOwnerWindow(parameter)
            };

            if (dlg.ShowDialog() == true)
            {
                GoldPrice = dlg.GoldPrice;
                SilverPrice = dlg.SilverPrice;
                PlatinumPrice = dlg.PlatinumPrice;
                PalladiumPrice = dlg.PalladiumPrice;
                BroncePrice = dlg.BroncePrice;
            }
        }

        public ICommand RefreshPricesCommand { get; }
        public ICommand ExportSimpleCommand { get; }
        public ICommand ExportDetailedCommand { get; }
        public ICommand ImportCommand { get; }
        public ICommand AddHoldingCommand { get; }
        public ICommand EditHoldingCommand { get; }
        public ICommand DeleteSelectedHoldingsCommand { get; }
        public ICommand EditPricesCommand { get; }
        public ICommand ToggleLanguageCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void ShowErrorMessage(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ExportSimpleHoldings()
        {
            var dateString = DateTime.Now.ToString(ExportFileDateFormat, CultureInfo.InvariantCulture);
            var exportFileName = $"{L("ExportButton")}_{dateString}.csv";

            var saveFileDialog = new SaveFileDialog
            {
                Filter = L("ExportDialog_Filter"),
                Title = L("ExportButton"),
                FileName = exportFileName
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var holdings = FilteredHoldings.Cast<MetalHolding>().ToList();
                if (holdings.Count == 0)
                {
                    MessageBox.Show(L("ExportDialog_NoHoldings"), L("ExportButton"), MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                try
                {
                    CsvExportService.ExportHoldings(holdings, saveFileDialog.FileName);
                    MessageBox.Show(L("ExportDialog_Success"), L("ExportButton"), MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{L("ExportDialog_Error")}: {ex.Message}", L("ExportButton"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportDetailedHoldings()
        {
            var dateString = DateTime.Now.ToString(ExportFileDateFormat, CultureInfo.InvariantCulture);
            var detailedSuffix = L("ExportDialog_Detailed");
            var exportFileName = $"{L("ExportButton")}_{dateString}_{detailedSuffix}.csv";

            var saveFileDialog = new SaveFileDialog
            {
                Filter = L("ExportDialog_Filter"),
                Title = L("ExportButton"),
                FileName = exportFileName
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var holdings = FilteredHoldings.Cast<MetalHolding>().ToList();
                if (holdings.Count == 0)
                {
                    MessageBox.Show(L("ExportDialog_NoHoldings"), L("ExportButton"), MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                try
                {
                    CsvExportService.ExportHoldingsDetailed(holdings, saveFileDialog.FileName);
                    MessageBox.Show(L("ExportDialog_Success"), L("ExportButton"), MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{L("ExportDialog_Error")}: {ex.Message}", L("ExportButton"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task ImportSimpleHoldingsAsync()
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    Filter = L("ImportDialog_Filter"),
                    Title = L("ImportDialog_Title")
                };

                if (dialog.ShowDialog() != true)
                    return;

                var lines = await System.IO.File.ReadAllLinesAsync(dialog.FileName);
                if (lines.Length == 0)
                    throw new InvalidOperationException(L("ImportDialog_NoData"));

                var newHoldings = new List<MetalHolding>();
                var lineNumber = 1;

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var values = line.Split(';');
                    if (values.Length < 8)
                        throw new FormatException($"{L("ImportDialog_InvalidFormat")} (Line {lineNumber})");

                    try
                    {
                        var holding = new MetalHolding
                        {
                            MetalType = (MetalType)int.Parse(values[0]),
                            Form = values[1],
                            CollectableType = (CollectableType)int.Parse(values[2]),
                            Purity = decimal.Parse(values[3]),
                            Weight = decimal.Parse(values[4]),
                            Quantity = int.Parse(values[5]),
                            PurchasePrice = decimal.Parse(values[6]),
                            PurchaseDate = DateTime.ParseExact(values[7], ImportExportCsvDateFormat, CultureInfo.InvariantCulture)
                        };

                        newHoldings.Add(holding);
                    }
                    catch (Exception)
                    {
                        throw new FormatException($"{L("ImportDialog_InvalidFormat")} (Line {lineNumber})");
                    }

                    lineNumber++;
                }

                // Asks user if they want to overwrite existing data if any data exists
                if (Holdings.Any())
                {
                    var result = MessageBox.Show(
                        L("ImportDialog_OverwritePrompt"),
                        L("ImportDialog_OverwritePrompt_Title"),
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question,
                        MessageBoxResult.No);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Deletes all existing data before import
                        foreach (var holding in Holdings.ToList())
                            DeleteHolding(holding);
                    }
                }

                foreach (var holding in newHoldings)
                    AddHolding(holding);

                MessageBox.Show(L("ImportDialog_Success"), L("ImportButton"), MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{L("ImportDialog_Error")}: {ex.Message}", L("ImportButton"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}