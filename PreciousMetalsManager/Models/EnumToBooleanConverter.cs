using System;
using System.Globalization;
using System.Windows.Data;

namespace PreciousMetalsManager.Models
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            string parameterString = parameter?.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(parameterString))
                return false;

            string valueString = value?.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(valueString))
                return false;

            return valueString.Equals(parameterString, StringComparison.InvariantCultureIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null || targetType == null)
                return Binding.DoNothing;

            if (value is bool boolean && boolean)
            {
                try
                {
                    return Enum.Parse(targetType, parameter.ToString()!);
                }
                catch
                {
                    return Binding.DoNothing;
                }
            }
            return Binding.DoNothing;
        }
    }
}