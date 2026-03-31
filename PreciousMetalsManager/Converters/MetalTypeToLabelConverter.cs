using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using PreciousMetalsManager.Domain;
using PreciousMetalsManager.Models;

namespace PreciousMetalsManager.Converters
{
    public sealed class MetalTypeToLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MetalType metalType)
            {
                return GetLabelWithoutColon(metalType);
            }

            if (value is string s)
            {
                var filterAll = Application.Current.TryFindResource("Filter_All") as string;
                if (!string.IsNullOrEmpty(filterAll) && string.Equals(s, filterAll, StringComparison.Ordinal))
                    return s;

                return s switch
                {
                    nameof(MetalType.Gold) => GetLabelWithoutColon(MetalType.Gold),
                    nameof(MetalType.Silver) => GetLabelWithoutColon(MetalType.Silver),
                    nameof(MetalType.Platinum) => GetLabelWithoutColon(MetalType.Platinum),
                    nameof(MetalType.Palladium) => GetLabelWithoutColon(MetalType.Palladium),
                    nameof(MetalType.Bronce) => GetLabelWithoutColon(MetalType.Bronce),
                    _ => s
                };
            }

            return value?.ToString() ?? string.Empty;
        }

        private static string GetLabelWithoutColon(MetalType metalType)
        {
            if (!DomainReferenceData.TryGetMetalLabelResourceKey(metalType, out var key))
                return metalType.ToString();

            var label = Application.Current.TryFindResource(key) as string;
            if (string.IsNullOrWhiteSpace(label))
                return metalType.ToString();

            return label.TrimEnd().TrimEnd(':');
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
