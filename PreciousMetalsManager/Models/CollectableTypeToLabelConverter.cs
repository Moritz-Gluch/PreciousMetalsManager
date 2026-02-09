using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using PreciousMetalsManager.Models;

namespace PreciousMetalsManager.Models
{
    public sealed class CollectableTypeToLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CollectableType collectableType)
            {
                return GetLabelWithoutColon(collectableType);
            }

            if (value is string s)
            {
                var filterAll = Application.Current.TryFindResource("Filter_All") as string;
                if (!string.IsNullOrEmpty(filterAll) && string.Equals(s, filterAll, StringComparison.Ordinal))
                    return s;

                return s switch
                {
                    nameof(CollectableType.Bullion) => GetLabelWithoutColon(CollectableType.Bullion),
                    nameof(CollectableType.SemiNumismatic) => GetLabelWithoutColon(CollectableType.SemiNumismatic),
                    nameof(CollectableType.Numismatic) => GetLabelWithoutColon(CollectableType.Numismatic),
                    _ => s
                };
            }

            return value?.ToString() ?? string.Empty;
        }

        private static string GetLabelWithoutColon(CollectableType collectableType)
        {
            var key = collectableType switch
            {
                CollectableType.Bullion => "CollectableType_Bullion",
                CollectableType.SemiNumismatic => "CollectableType_SemiNumismatic",
                CollectableType.Numismatic => "CollectableType_Numismatic",
                _ => null
            };

            if (key is null)
                return collectableType.ToString();

            var label = Application.Current.TryFindResource(key) as string;
            if (string.IsNullOrWhiteSpace(label))
                return collectableType.ToString();

            return label.TrimEnd().TrimEnd(':');
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}