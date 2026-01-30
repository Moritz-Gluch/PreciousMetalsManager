using System;
using System.Linq;
using System.Windows;

namespace PreciousMetalsManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string LocalizationFolder = "Resources/Localization/";
        public static string CurrentLanguage { get; private set; } = "en";

        public static void SetLanguage(string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
                throw new ArgumentException("languageCode must not be null/empty.", nameof(languageCode));

            languageCode = languageCode.ToLowerInvariant();

            var source = languageCode switch
            {
                "de" => new Uri($"{LocalizationFolder}Localization.de.xaml", UriKind.Relative),
                "en" => new Uri($"{LocalizationFolder}Localization.en.xaml", UriKind.Relative),
                _ => throw new NotSupportedException($"Language '{languageCode}' is not supported.")
            };

            var newDict = new ResourceDictionary { Source = source };

            var merged = Current.Resources.MergedDictionaries;

            var oldDict = merged.FirstOrDefault(d =>
                d.Source != null &&
                d.Source.OriginalString.StartsWith(LocalizationFolder, StringComparison.OrdinalIgnoreCase) &&
                d.Source.OriginalString.Contains("Localization.", StringComparison.OrdinalIgnoreCase));

            if (oldDict != null)
                merged.Remove(oldDict);

            merged.Add(newDict);
            CurrentLanguage = languageCode;
        }
    }
}
