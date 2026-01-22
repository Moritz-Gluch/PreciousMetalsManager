using System;
using System.Collections.ObjectModel;
using PreciousMetalsManager.Models;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections.Generic;
using System.Linq;

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

        public ViewModel()
        {
            Holdings = new ObservableCollection<MetalHolding>();

            // Example-Data for In-Memory-CRUD
            Holdings.Add(new MetalHolding
            {
                MetalType = MetalType.Gold,
                Form = "Bar",
                Purity = 999.9m,
                Weight = 100m,
                Quantity = 1,
                PurchasePrice = 5800m,
                PurchaseDate = DateTime.Now,
                CurrentValue = 6000m,
                TotalValue = 6000m
            });

            Holdings.Add(new MetalHolding
            {
                MetalType = MetalType.Silver,
                Form = "Bar",
                Purity = 625m,
                Weight = 100m,
                Quantity = 1,
                PurchasePrice = 25m,
                PurchaseDate = DateTime.Now,
                CurrentValue = 30m,
                TotalValue = 30m
            });

            Holdings.Add(new MetalHolding
            {
                MetalType = MetalType.Palladium,
                Form = "Coin",
                Purity = 999.9m,
                Weight = 50m,
                Quantity = 1,
                PurchasePrice = 5800m,
                PurchaseDate = DateTime.Now,
                CurrentValue = 500m,
                TotalValue = 6400m
            });

            Holdings.Add(new MetalHolding
            {
                MetalType = MetalType.Platinum,
                Form = "Coin",
                Purity = 999.9m,
                Weight = 190m,
                Quantity = 1,
                PurchasePrice = 500m,
                PurchaseDate = DateTime.Now,
                CurrentValue = 600m,
                TotalValue = 600m
            });

            FilteredHoldings = CollectionViewSource.GetDefaultView(Holdings);
            FilteredHoldings.Filter = FilterPredicate;
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}