using Microsoft.VisualStudio.TestTools.UnitTesting;
using PreciousMetalsManager.Models;
using PreciousMetalsManager.Services;
using PreciousMetalsManager.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PreciousMetalsManager.Tests
{
    [STATestClass]
    public class ViewModelTest
    {
        private string _testDbPath = null!;
        private ViewModel _vm = null!;
        private RecordingMessageService _messageService = null!;
        private StubLanguageService _languageService = null!;
        private StubTextProvider _textProvider = null!;

        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            EnsureWpfApp();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _testDbPath = Path.Combine(Path.GetTempPath(), $"test_holdings_{Guid.NewGuid()}.db");
            _messageService = new RecordingMessageService();
            _languageService = new StubLanguageService();
            _textProvider = new StubTextProvider(_languageService);
            _vm = CreateTestViewModel();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _vm = null!;
            GC.Collect();
            GC.WaitForPendingFinalizers();

            if (File.Exists(_testDbPath))
            {
                try
                {
                    File.Delete(_testDbPath);
                }
                catch (IOException ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error deleting test database: " + ex.Message);
                }
            }
        }

        [TestMethod]
        public void AddHolding_AddsItemToHoldings()
        {
            var holding = CreateTestHolding(form: "Barren", metalType: MetalType.Gold);

            _vm.AddHolding(holding);

            Assert.IsTrue(_vm.Holdings.Any(h => h.Form == "Barren" && h.MetalType == MetalType.Gold));
        }

        [TestMethod]
        public void DeleteHolding_RemovesItemFromHoldings()
        {
            var holding = CreateTestHolding(form: "Münze", metalType: MetalType.Silver);
            _vm.AddHolding(holding);

            Assert.IsTrue(_vm.Holdings.Any(h => h.Id == holding.Id));

            _vm.DeleteHolding(holding);

            Assert.IsFalse(_vm.Holdings.Any(h => h.Id == holding.Id));
        }

        [TestMethod]
        public void FormFilter_FiltersHoldings()
        {
            var bar = CreateTestHolding(form: "Barren", metalType: MetalType.Gold);
            var coin = CreateTestHolding(form: "Münze", metalType: MetalType.Silver);

            _vm.AddHolding(bar);
            _vm.AddHolding(coin);

            _vm.FormFilter = "Bar";
            var filtered = _vm.FilteredHoldings.Cast<MetalHolding>().ToList();

            Assert.HasCount(1, filtered);
            Assert.AreEqual("Barren", filtered[0].Form);
        }

        [TestMethod]
        public void SelectedMetalTypeFilter_FiltersHoldings()
        {
            _vm.AddHolding(CreateTestHolding(form: "Gold-Bar", metalType: MetalType.Gold));
            _vm.AddHolding(CreateTestHolding(form: "Silver-Coin", metalType: MetalType.Silver));

            _vm.SelectedMetalTypeFilter = _vm.MetalTypeFilterOptions.Single(o => o.Value == MetalType.Gold);
            var filtered = _vm.FilteredHoldings.Cast<MetalHolding>().ToList();

            Assert.HasCount(1, filtered);
            Assert.AreEqual(MetalType.Gold, filtered[0].MetalType);
        }

        [TestMethod]
        public void SelectedCollectableTypeFilter_FiltersHoldings()
        {
            _vm.AddHolding(CreateTestHolding(form: "Bullion", collectableType: CollectableType.Bullion));
            _vm.AddHolding(CreateTestHolding(form: "Numismatic", collectableType: CollectableType.Numismatic));

            _vm.SelectedCollectableTypeFilter = _vm.CollectableTypeFilterOptions.Single(o => o.Value == CollectableType.Numismatic);
            var filtered = _vm.FilteredHoldings.Cast<MetalHolding>().ToList();

            Assert.HasCount(1, filtered);
            Assert.AreEqual(CollectableType.Numismatic, filtered[0].CollectableType);
        }

        [TestMethod]
        public void PropertyChanged_IsRaised_WhenGoldPriceChanges()
        {
            var raised = false;
            _vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(ViewModel.GoldPrice))
                    raised = true;
            };

            _vm.GoldPrice = 10m;

            Assert.IsTrue(raised);
        }

        [TestMethod]
        public void UpdateCalculatedValues_UpdatesCurrentAndTotalValue()
        {
            var holding = CreateTestHolding(
                metalType: MetalType.Gold,
                weight: 2m,
                purity: 999.9m,
                quantity: 3);

            _vm.AddHolding(holding);
            var persistedHolding = _vm.Holdings.Single(h => h.Id == holding.Id);

            _vm.GoldPrice = 10m;

            Assert.AreEqual(20m, persistedHolding.CurrentValue);
            Assert.AreEqual(60m, persistedHolding.TotalValue);
        }

        [TestMethod]
        public async Task UpdateMarketPricesAsync_UpdatesPricesAndRecalculatesHoldings()
        {
            var apiService = new TestMetalPriceApiService
            {
                ResponseToReturn = new MetalPriceApiResponse
                {
                    GoldEur = 3110m,
                    SilverEur = 62.2m,
                    PlatinumEur = 155.5m,
                    PalladiumEur = 62.2m
                }
            };

            var vm = CreateTestViewModel(apiService: apiService);
            var holding = CreateTestHolding(
                metalType: MetalType.Gold,
                weight: 2m,
                purity: 999.9m,
                quantity: 1);

            vm.AddHolding(holding);
            var persistedHolding = vm.Holdings.Single(h => h.Id == holding.Id);

            await vm.UpdateMarketPricesAsync();

            Assert.AreEqual(100.00m, vm.GoldPrice);
            Assert.AreEqual(2.00m, vm.SilverPrice);
            Assert.AreEqual(5.00m, vm.PlatinumPrice);
            Assert.AreEqual(2.00m, vm.PalladiumPrice);
            Assert.AreEqual(200.00m, persistedHolding.CurrentValue);
            Assert.AreEqual(200.00m, persistedHolding.TotalValue);
        }

        [TestMethod]
        public async Task UpdateMarketPricesAsync_ApiError_ShowsErrorMessage()
        {
            var messageService = new RecordingMessageService();
            var apiService = new TestMetalPriceApiService { ResponseToReturn = null };
            var vm = CreateTestViewModel(apiService: apiService, messageService: messageService);

            messageService.Clear();

            await vm.UpdateMarketPricesAsync();

            Assert.AreEqual("Price API error", messageService.LastErrorMessage);
            Assert.AreEqual("Error", messageService.LastErrorTitle);
        }

        [TestMethod]
        public void MetalTypeFilterOptions_FirstEntry_IsAllOption()
        {
            Assert.IsGreaterThanOrEqualTo(_vm.MetalTypeFilterOptions.Count, 1);

            var first = _vm.MetalTypeFilterOptions[0];
            Assert.IsNull(first.Value);
            Assert.AreEqual("All", first.DisplayText);
            Assert.AreSame(first, _vm.SelectedMetalTypeFilter);
        }

        [TestMethod]
        public void CollectableTypeFilterOptions_FirstEntry_IsAllOption()
        {
            Assert.IsGreaterThanOrEqualTo(_vm.CollectableTypeFilterOptions.Count, 1);

            var first = _vm.CollectableTypeFilterOptions[0];
            Assert.IsNull(first.Value);
            Assert.AreEqual("All", first.DisplayText);
            Assert.AreSame(first, _vm.SelectedCollectableTypeFilter);
        }

        [TestMethod]
        public void TaxFreeOnly_Filter_WorksCorrectly()
        {
            var oldHolding = CreateTestHolding(purchaseDate: DateTime.Today.AddYears(-2), form: "Old");
            var youngHolding = CreateTestHolding(purchaseDate: DateTime.Today.AddMonths(-6), form: "Young");

            _vm.AddHolding(oldHolding);
            _vm.AddHolding(youngHolding);

            _vm.TaxFreeOnly = true;
            var filtered = _vm.FilteredHoldings.Cast<MetalHolding>().ToList();

            Assert.HasCount(1, filtered);
            Assert.AreEqual("Old", filtered[0].Form);
        }

        [TestMethod]
        public void AddHoldingCommand_UsesHoldingDialogServiceAndAddsReturnedHolding()
        {
            var dialogHolding = CreateTestHolding(form: "DialogHolding");
            var holdingDialogService = new StubHoldingDialogService
            {
                AddResult = new HoldingDialogResult(true, dialogHolding, false)
            };

            var vm = CreateTestViewModel(holdingDialogService: holdingDialogService);

            vm.AddHoldingCommand.Execute(null);

            Assert.AreEqual(1, holdingDialogService.AddCallCount);
            Assert.IsTrue(vm.Holdings.Any(h => h.Form == "DialogHolding"));
        }

        [TestMethod]
        public void EditHoldingCommand_UsesHoldingDialogServiceAndUpdatesSelectedHolding()
        {
            var original = CreateTestHolding(form: "Original", collectableType: CollectableType.Bullion);
            _vm.AddHolding(original);

            var persisted = _vm.Holdings.Single(h => h.Id == original.Id);
            _vm.SelectedHolding = persisted;

            var edited = CreateTestHolding(form: "Edited", collectableType: CollectableType.Numismatic);
            edited.Id = persisted.Id;

            var holdingDialogService = new StubHoldingDialogService
            {
                EditResult = new HoldingDialogResult(true, edited, false)
            };

            var vm = CreateTestViewModel(holdingDialogService: holdingDialogService);
            vm.AddHolding(original);
            var selected = vm.Holdings.Single(h => h.Id == original.Id);
            vm.SelectedHolding = selected;

            vm.EditHoldingCommand.Execute(null);

            Assert.AreEqual(1, holdingDialogService.EditCallCount);
            Assert.IsTrue(vm.Holdings.Any(h => h.Id == selected.Id && h.Form == "Edited" && h.CollectableType == CollectableType.Numismatic));
        }

        [TestMethod]
        public void DeleteSelectedHoldingsCommand_ConfirmsAndDeletesSelectedItems()
        {
            var first = CreateTestHolding(form: "First");
            var second = CreateTestHolding(form: "Second");
            _vm.AddHolding(first);
            _vm.AddHolding(second);

            _messageService.ShowConfirmationResult = true;
            _vm.UpdateSelection(_vm.Holdings.ToList());

            _vm.DeleteSelectedHoldingsCommand.Execute(null);

            Assert.IsEmpty(_vm.Holdings);
            Assert.IsGreaterThanOrEqualTo(_messageService.ConfirmationRequests.Count, 1);
        }

        [TestMethod]
        public void EditPricesCommand_AppliesDialogResult()
        {
            var editPricesDialogService = new StubEditPricesDialogService
            {
                Result = new PriceEditResult(100m, 2m, 5m, 3m, 1m)
            };

            var vm = CreateTestViewModel(editPricesDialogService: editPricesDialogService);

            vm.EditPricesCommand.Execute(null);

            Assert.AreEqual(100m, vm.GoldPrice);
            Assert.AreEqual(2m, vm.SilverPrice);
            Assert.AreEqual(5m, vm.PlatinumPrice);
            Assert.AreEqual(3m, vm.PalladiumPrice);
            Assert.AreEqual(1m, vm.BroncePrice);
        }

        [TestMethod]
        public void ToggleLanguage_UpdatesAllOptionText()
        {
            Assert.AreEqual("All", _vm.MetalTypeFilterOptions[0].DisplayText);

            _vm.ToggleLanguage();

            Assert.AreEqual("Alle", _vm.MetalTypeFilterOptions[0].DisplayText);
        }

        [TestMethod]
        public void ToggleLanguageCommand_RaisesLanguageLayoutRefreshRequested()
        {
            var raised = false;
            _vm.LanguageLayoutRefreshRequested += (_, _) => raised = true;

            _vm.ToggleLanguageCommand.Execute(null);

            Assert.IsTrue(raised);
        }

        [TestMethod]
        public void UpdateSelection_WithSingleItem_SetsSelectedHoldingAndHasSelection()
        {
            var holding = CreateTestHolding(form: "Single");
            _vm.AddHolding(holding);

            var selected = _vm.Holdings.Single(h => h.Form == "Single");

            _vm.UpdateSelection(new[] { selected });

            Assert.AreSame(selected, _vm.SelectedHolding);
            Assert.HasCount(1, _vm.SelectedHoldings);
            Assert.IsTrue(_vm.HasSelection);
            Assert.IsTrue(_vm.HasSingleSelection);
        }

        [TestMethod]
        public void UpdateSelection_WithEmptySelection_ClearsSelectedHolding()
        {
            var holding = CreateTestHolding(form: "Single");
            _vm.AddHolding(holding);

            var selected = _vm.Holdings.Single(h => h.Form == "Single");
            _vm.UpdateSelection(new[] { selected });

            _vm.UpdateSelection(Array.Empty<MetalHolding>());

            Assert.IsNull(_vm.SelectedHolding);
            Assert.IsEmpty(_vm.SelectedHoldings);
            Assert.IsFalse(_vm.HasSelection);
            Assert.IsFalse(_vm.HasSingleSelection);
        }

        [TestMethod]
        public void EditHoldingCommand_WithoutSelection_ShowsInformationMessage()
        {
            _messageService.Clear();

            _vm.EditHoldingCommand.Execute(null);

            Assert.HasCount(1, _messageService.InformationRequests);
        }

        [TestMethod]
        public void DeleteSelectedHoldingsCommand_WhenConfirmationIsDeclined_DoesNotDeleteItems()
        {
            var first = CreateTestHolding(form: "First");
            var second = CreateTestHolding(form: "Second");
            _vm.AddHolding(first);
            _vm.AddHolding(second);

            _messageService.ShowConfirmationResult = false;
            _vm.UpdateSelection(_vm.Holdings.ToList());

            _vm.DeleteSelectedHoldingsCommand.Execute(null);

            Assert.HasCount(2, _vm.Holdings);
            Assert.HasCount(1, _messageService.ConfirmationRequests);
        }

        [TestMethod]
        public void EditPricesCommand_WhenDialogReturnsNull_DoesNotChangePrices()
        {
            var editPricesDialogService = new StubEditPricesDialogService
            {
                Result = null
            };

            var vm = CreateTestViewModel(editPricesDialogService: editPricesDialogService);
            vm.GoldPrice = 10m;
            vm.SilverPrice = 2m;
            vm.PlatinumPrice = 3m;
            vm.PalladiumPrice = 4m;
            vm.BroncePrice = 1m;

            vm.EditPricesCommand.Execute(null);

            Assert.AreEqual(10m, vm.GoldPrice);
            Assert.AreEqual(2m, vm.SilverPrice);
            Assert.AreEqual(3m, vm.PlatinumPrice);
            Assert.AreEqual(4m, vm.PalladiumPrice);
            Assert.AreEqual(1m, vm.BroncePrice);
        }

        [TestMethod]
        public void DeleteSelectedHoldingsCommand_WithoutSelection_ShowsInformationMessage()
        {
            _messageService.Clear();

            _vm.DeleteSelectedHoldingsCommand.Execute(null);

            Assert.HasCount(1, _messageService.InformationRequests);
            Assert.IsEmpty(_messageService.ConfirmationRequests);
            Assert.IsEmpty(_vm.Holdings);
        }

        [TestMethod]
        public void UpdateSelectionCommand_WithNonEnumerableParameter_ClearsSelection()
        {
            var holding = CreateTestHolding(form: "Selected");
            _vm.AddHolding(holding);

            var selected = _vm.Holdings.Single(h => h.Form == "Selected");
            _vm.UpdateSelection(new[] { selected });

            _vm.UpdateSelectionCommand.Execute(new object());

            Assert.IsNull(_vm.SelectedHolding);
            Assert.IsEmpty(_vm.SelectedHoldings);
            Assert.IsFalse(_vm.HasSelection);
            Assert.IsFalse(_vm.HasSingleSelection);
        }

        [TestMethod]
        public void AddHoldingCommand_WhenAddAnotherRequested_RepeatsUntilCancelled()
        {
            var firstHolding = CreateTestHolding(form: "FirstDialogHolding");
            var secondHolding = CreateTestHolding(form: "SecondDialogHolding");

            var holdingDialogService = new StubHoldingDialogService();
            holdingDialogService.AddResults.Enqueue(new HoldingDialogResult(true, firstHolding, true));
            holdingDialogService.AddResults.Enqueue(new HoldingDialogResult(true, secondHolding, false));

            var vm = CreateTestViewModel(holdingDialogService: holdingDialogService);

            vm.AddHoldingCommand.Execute(null);

            Assert.AreEqual(2, holdingDialogService.AddCallCount);
            Assert.IsTrue(vm.Holdings.Any(h => h.Form == "FirstDialogHolding"));
            Assert.IsTrue(vm.Holdings.Any(h => h.Form == "SecondDialogHolding"));
            Assert.HasCount(2, vm.Holdings);
        }

        private ViewModel CreateTestViewModel(
            TestMetalPriceApiService? apiService = null,
            RecordingMessageService? messageService = null,
            StubLanguageService? languageService = null,
            StubTextProvider? textProvider = null,
            StubHoldingDialogService? holdingDialogService = null,
            StubEditPricesDialogService? editPricesDialogService = null)
        {
            var effectiveLanguageService = languageService ?? _languageService;
            var effectiveTextProvider = textProvider ?? new StubTextProvider(effectiveLanguageService);
            var effectiveMessageService = messageService ?? _messageService;

            var storage = new LocalStorageService(_testDbPath, effectiveMessageService, effectiveTextProvider);
            var fileDialogService = new StubFileDialogService();

            return new ViewModel(
                storage,
                apiService ?? new TestMetalPriceApiService
                {
                    ResponseToReturn = new MetalPriceApiResponse()
                },
                effectiveMessageService,
                fileDialogService,
                effectiveLanguageService,
                effectiveTextProvider,
                holdingDialogService ?? new StubHoldingDialogService(),
                editPricesDialogService ?? new StubEditPricesDialogService());
        }

        private static MetalHolding CreateTestHolding(
            string form = "Barren",
            MetalType metalType = MetalType.Gold,
            CollectableType collectableType = CollectableType.Bullion,
            decimal purity = 999.9m,
            decimal weight = 1m,
            int quantity = 1,
            decimal purchasePrice = 100m,
            DateTime? purchaseDate = null)
        {
            return new MetalHolding
            {
                MetalType = metalType,
                Form = form,
                CollectableType = collectableType,
                Purity = purity,
                Weight = weight,
                Quantity = quantity,
                PurchasePrice = purchasePrice,
                PurchaseDate = purchaseDate ?? DateTime.Today
            };
        }

        private static void EnsureWpfApp()
        {
            if (Application.Current is not null)
                return;

            _ = new Application
            {
                ShutdownMode = ShutdownMode.OnExplicitShutdown
            };
        }

        private sealed class TestMetalPriceApiService : MetalPriceApiService
        {
            public MetalPriceApiResponse? ResponseToReturn { get; set; }

            public override Task<MetalPriceApiResponse?> FetchMetalPricesAsync()
                => Task.FromResult(ResponseToReturn);
        }

        private sealed class RecordingMessageService : IMessageService
        {
            public List<(string Message, string? Title)> InformationRequests { get; } = new();
            public List<(string Message, string Title)> WarningRequests { get; } = new();
            public List<(string Message, string Title)> ErrorRequests { get; } = new();
            public List<(string Message, string Title)> ConfirmationRequests { get; } = new();

            public bool ShowConfirmationResult { get; set; } = true;

            public string? LastErrorMessage => ErrorRequests.LastOrDefault().Message;
            public string? LastErrorTitle => ErrorRequests.LastOrDefault().Title;

            public void Clear()
            {
                InformationRequests.Clear();
                WarningRequests.Clear();
                ErrorRequests.Clear();
                ConfirmationRequests.Clear();
            }

            public void ShowInformation(string message, string? title = null)
                => InformationRequests.Add((message, title));

            public void ShowWarning(string message, string title)
                => WarningRequests.Add((message, title));

            public void ShowError(string message, string title)
                => ErrorRequests.Add((message, title));

            public bool ShowConfirmation(string message, string title)
            {
                ConfirmationRequests.Add((message, title));
                return ShowConfirmationResult;
            }
        }

        private sealed class StubFileDialogService : IFileDialogService
        {
            public string? ShowOpenFileDialog(string filter, string title) => null;

            public string? ShowSaveFileDialog(string filter, string title, string fileName) => null;
        }

        private sealed class StubLanguageService : ILanguageService
        {
            public string CurrentLanguage { get; private set; } = "en";

            public void ToggleLanguage()
                => CurrentLanguage = CurrentLanguage == "en" ? "de" : "en";
        }

        private sealed class StubTextProvider : ITextProvider
        {
            private readonly StubLanguageService _languageService;

            public StubTextProvider(StubLanguageService languageService)
            {
                _languageService = languageService;
            }

            public string GetString(string key)
            {
                return (_languageService.CurrentLanguage, key) switch
                {
                    ("de", "Filter_All") => "Alle",
                    ("en", "Filter_All") => "All",
                    ("de", "Msg_PriceApiError") => "Preis-API-Fehler",
                    ("en", "Msg_PriceApiError") => "Price API error",
                    ("de", "Msg_ErrorTitle") => "Fehler",
                    ("en", "Msg_ErrorTitle") => "Error",
                    ("de", "Lbl_Gold") => "Gold:",
                    ("en", "Lbl_Gold") => "Gold:",
                    ("de", "Lbl_Silver") => "Silber:",
                    ("en", "Lbl_Silver") => "Silver:",
                    ("de", "Lbl_Platinum") => "Platin:",
                    ("en", "Lbl_Platinum") => "Platinum:",
                    ("de", "Lbl_Palladium") => "Palladium:",
                    ("en", "Lbl_Palladium") => "Palladium:",
                    ("de", "Lbl_Bronce") => "Bronze:",
                    ("en", "Lbl_Bronce") => "Bronze:",
                    ("de", "CollectableType_Bullion") => "Anlage",
                    ("en", "CollectableType_Bullion") => "Bullion",
                    ("de", "CollectableType_SemiNumismatic") => "Semi-numismatisch",
                    ("en", "CollectableType_SemiNumismatic") => "Semi-numismatic",
                    ("de", "CollectableType_Numismatic") => "Numismatisch",
                    ("en", "CollectableType_Numismatic") => "Numismatic",
                    _ => key
                };
            }
        }

        private sealed class StubHoldingDialogService : IHoldingDialogService
        {
            public HoldingDialogResult AddResult { get; set; } = HoldingDialogResult.Cancelled;
            public HoldingDialogResult EditResult { get; set; } = HoldingDialogResult.Cancelled;

            public Queue<HoldingDialogResult> AddResults { get; } = new();

            public int AddCallCount { get; private set; }
            public int EditCallCount { get; private set; }

            public HoldingDialogResult ShowAddDialog(ViewModel viewModel)
            {
                AddCallCount++;

                if (AddResults.Count > 0)
                    return AddResults.Dequeue();

                return AddResult;
            }

            public HoldingDialogResult ShowEditDialog(ViewModel viewModel, MetalHolding holding)
            {
                EditCallCount++;
                return EditResult;
            }
        }

        private sealed class StubEditPricesDialogService : IEditPricesDialogService
        {
            public PriceEditResult? Result { get; set; }

            public PriceEditResult? ShowEditPricesDialog(
                decimal goldPrice,
                decimal silverPrice,
                decimal platinumPrice,
                decimal palladiumPrice,
                decimal broncePrice,
                string priceUnit)
            {
                return Result;
            }
        }
    }
}
