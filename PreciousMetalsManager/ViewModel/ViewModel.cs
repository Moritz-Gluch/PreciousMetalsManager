using System;
using System.Collections.ObjectModel;
using PreciousMetalsManager.Models;

namespace PreciousMetalsManager.ViewModels
{
    public class ViewModel
    {
        public ObservableCollection<MetalHolding> Holdings { get; }

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
        }
    }
}