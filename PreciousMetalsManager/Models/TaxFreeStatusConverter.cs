using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PreciousMetalsManager.Models
{
    public sealed class TaxFreeStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not MetalHolding holding)
                return string.Empty;

            if (holding.PurchaseDate == default)
                return string.Empty;

            if (holding.IsTaxFree)
                return L("TaxFreeStatus_Yes");

            return $"{holding.TaxFreeDaysLeft} {L("TaxFreeStatus_DaysLeft")}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;

        private static string L(string key)
            => Application.Current?.TryFindResource(key) as string ?? key;
    }
}