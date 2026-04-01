using System;
using System.Collections;
using System.Collections.ObjectModel;
using PreciousMetalsManager.Models;
using System.ComponentModel;
using System.Windows.Data;
using System.Linq;
using System.Collections.Specialized;
using PreciousMetalsManager.Services;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using System.Globalization;
using PreciousMetalsManager.Domain;
using System.Collections.Generic;

namespace PreciousMetalsManager.ViewModels
{
    public class ViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<MetalHolding> Holdings { get; }
        public ICollectionView FilteredHoldings { get; }

        private readonly LocalStorageService _storage;
        private readonly MetalPriceApiService _metalPriceApiService;
        private readonly IMessageService _messageService;
        private readonly IFileDialogService _fileDialogService;
        private readonly ILanguageService _languageService;
        private readonly ITextProvider _textProvider;
        private readonly IHoldingDialogService _holdingDialogService;
        private readonly IEditPricesDialogService _editPricesDialogService;

        private string L(string key)
            => _textProvider.GetString(key);

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

        public ViewModel(
            LocalStorageService storage,
            MetalPriceApiService metalPriceApiService,
            IMessageService messageService,
            IFileDialogService fileDialogService,
            ILanguageService languageService,
            ITextProvider textProvider,
            IHoldingDialogService holdingDialogService,
            IEditPricesDialogService editPricesDialogService)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _metalPriceApiService = metalPriceApiService ?? throw new ArgumentNullException(nameof(metalPriceApiService));
            _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
            _fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
            _languageService = languageService ?? throw new ArgumentNullException(nameof(languageService));
            _textProvider = textProvider ?? throw new ArgumentNullException(nameof(textProvider));
            _holdingDialogService = holdingDialogService ?? throw new ArgumentNullException(nameof(holdingDialogService));
            _editPricesDialogService = editPricesDialogService ?? throw new ArgumentNullException(nameof(editPricesDialogService));

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
            UpdateSelectionCommand = new RelayCommand(ExecuteUpdateSelection);
        }

        private void ExecuteUpdateSelection(object? parameter)
        {
            if (parameter is IEnumerable items)
            {
                UpdateSelection(items.OfType<MetalHolding>());
                return;
            }

            UpdateSelection(Array.Empty<MetalHolding>());
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
            bool wasAllMetalType = SelectedMetalTypeFilter is string;
            bool wasAllCollectableType = SelectedCollectableTypeFilter is string;

            var oldMetalType = SelectedMetalTypeFilter;
            var oldCollectableType = SelectedCollectableTypeFilter;

            _languageService.ToggleLanguage();
            UpdateFilterOptions(resetSelection: false);

            var allOption = L("Filter_All");
            if (wasAllMetalType)
                SelectedMetalTypeFilter = allOption;
            else
            {
                // Needed Workaround to trigger filter refresh
                SelectedMetalTypeFilter = null;
                SelectedMetalTypeFilter = oldMetalType;
            }

            if (wasAllCollectableType)
                SelectedCollectableTypeFilter = allOption;
            else
            {
                // Needed Workaround to trigger filter refresh
                SelectedCollectableTypeFilter = null;
                SelectedCollectableTypeFilter = oldCollectableType;
            }

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

        private void ExecuteAddHolding(object? parameter)
        {
            var keepAdding = true;
            while (keepAdding)
            {
                var result = _holdingDialogService.ShowAddDialog(this);
                if (result.Accepted && result.Holding is { } newHolding)
                {
                    AddHolding(newHolding);
                    keepAdding = result.AddAnotherRequested;
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
                _messageService.ShowInformation(L("Msg_SelectHoldingToEdit"));
                return;
            }

            var result = _holdingDialogService.ShowEditDialog(this, selected);
            if (result.Accepted && result.Holding is { } edited)
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
                _messageService.ShowInformation(L("Msg_SelectHoldingToDelete"));
                return;
            }

            var confirmed = selectedItems.Count == 1
                ? _messageService.ShowConfirmation(
                    L("Msg_ConfirmDeleteText"),
                    L("Msg_ConfirmDeleteTitle"))
                : _messageService.ShowConfirmation(
                    string.Format(L("Msg_ConfirmDeleteMultipleText"), selectedItems.Count),
                    L("Msg_ConfirmDeleteTitle"));

            if (confirmed)
            {
                foreach (var holding in selectedItems)
                    DeleteHolding(holding);

                UpdateSelection(Array.Empty<MetalHolding>());
            }
        }

        private void ExecuteEditPrices(object? parameter)
        {
            var result = _editPricesDialogService.ShowEditPricesDialog(
                GoldPrice,
                SilverPrice,
                PlatinumPrice,
                PalladiumPrice,
                BroncePrice,
                PriceUnit);

            if (result is null)
                return;

            GoldPrice = result.GoldPrice;
            SilverPrice = result.SilverPrice;
            PlatinumPrice = result.PlatinumPrice;
            PalladiumPrice = result.PalladiumPrice;
            BroncePrice = result.BroncePrice;
        }

        private DetailedExportTexts CreateDetailedExportTexts()
        {
            return new DetailedExportTexts
            {
                MetalTypeHeader = L("Common_MetalType"),
                FormHeader = L("Common_Form"),
                CollectableTypeHeader = L("Common_CollectableType"),
                PurityHeader = L("Common_Purity"),
                WeightHeader = L("Common_Weight"),
                QuantityHeader = L("Common_Quantity"),
                PurchasePriceHeader = L("Common_PurchasePrice"),
                PurchaseDateHeader = L("Common_PurchaseDate"),

                GoldLabel = L("Lbl_Gold").TrimEnd().TrimEnd(':'),
                SilverLabel = L("Lbl_Silver").TrimEnd().TrimEnd(':'),
                BronceLabel = L("Lbl_Bronce").TrimEnd().TrimEnd(':'),
                PlatinumLabel = L("Lbl_Platinum").TrimEnd().TrimEnd(':'),
                PalladiumLabel = L("Lbl_Palladium").TrimEnd().TrimEnd(':'),

                BullionLabel = L("CollectableType_Bullion"),
                SemiNumismaticLabel = L("CollectableType_SemiNumismatic"),
                NumismaticLabel = L("CollectableType_Numismatic")
            };
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
        public ICommand UpdateSelectionCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void ShowErrorMessage(string message, string title)
        {
            _messageService.ShowError(message, title);
        }

        private void ExportSimpleHoldings()
        {
            var dateString = DateTime.Now.ToString(ExportFileDateFormat, CultureInfo.InvariantCulture);
            var exportFileName = $"{L("ExportButton")}_{dateString}.csv";

            var filePath = _fileDialogService.ShowSaveFileDialog(
                L("ExportDialog_Filter"),
                L("ExportButton"),
                exportFileName);

            if (string.IsNullOrWhiteSpace(filePath))
                return;

            var holdings = FilteredHoldings.Cast<MetalHolding>().ToList();
            if (holdings.Count == 0)
            {
                _messageService.ShowInformation(L("ExportDialog_NoHoldings"), L("ExportButton"));
                return;
            }

            try
            {
                CsvExportService.ExportHoldings(holdings, filePath);
                _messageService.ShowInformation(L("ExportDialog_Success"), L("ExportButton"));
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"{L("ExportDialog_Error")}: {ex.Message}", L("ExportButton"));
            }
        }

        private void ExportDetailedHoldings()
        {
            var dateString = DateTime.Now.ToString(ExportFileDateFormat, CultureInfo.InvariantCulture);
            var detailedSuffix = L("ExportDialog_Detailed");
            var exportFileName = $"{L("ExportButton")}_{dateString}_{detailedSuffix}.csv";

            var filePath = _fileDialogService.ShowSaveFileDialog(
                L("ExportDialog_Filter"),
                L("ExportButton"),
                exportFileName);

            if (string.IsNullOrWhiteSpace(filePath))
                return;

            var holdings = FilteredHoldings.Cast<MetalHolding>().ToList();
            if (holdings.Count == 0)
            {
                _messageService.ShowInformation(L("ExportDialog_NoHoldings"), L("ExportButton"));
                return;
            }

            try
            {
                CsvExportService.ExportHoldingsDetailed(holdings, filePath, CreateDetailedExportTexts());
                _messageService.ShowInformation(L("ExportDialog_Success"), L("ExportButton"));
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"{L("ExportDialog_Error")}: {ex.Message}", L("ExportButton"));
            }
        }

        private async Task ImportSimpleHoldingsAsync()
        {
            try
            {
                var filePath = _fileDialogService.ShowOpenFileDialog(
                    L("ImportDialog_Filter"),
                    L("ImportDialog_Title"));

                if (string.IsNullOrWhiteSpace(filePath))
                    return;

                var lines = await System.IO.File.ReadAllLinesAsync(filePath);
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
                    var overwrite = _messageService.ShowConfirmation(
                        L("ImportDialog_OverwritePrompt"),
                        L("ImportDialog_OverwritePrompt_Title"));

                    if (overwrite)
                    {
                        // Deletes all existing data before import
                        foreach (var holding in Holdings.ToList())
                            DeleteHolding(holding);
                    }
                }

                foreach (var holding in newHoldings)
                    AddHolding(holding);

                _messageService.ShowInformation(L("ImportDialog_Success"), L("ImportButton"));
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"{L("ImportDialog_Error")}: {ex.Message}", L("ImportButton"));
            }
        }
    }
}