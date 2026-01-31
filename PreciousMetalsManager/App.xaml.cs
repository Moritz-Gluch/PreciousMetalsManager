using System;
using System.Linq;
using System.Windows;
using PreciousMetalsManager.Services;

namespace PreciousMetalsManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string LocalizationFolder = "Resources/Localization/";
        public static string CurrentLanguage { get; private set; } = "en";

        private static readonly LanguagePreferenceStore LanguageStore = new();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var savedLanguage = LanguageStore.TryLoad();
            if (!string.IsNullOrWhiteSpace(savedLanguage))
            {
                SetLanguage(savedLanguage);
            }
            else
            {
                SetLanguage(CurrentLanguage);
            }
        }

        public static void SetLanguage(string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
                throw new ArgumentException("languageCode must not be null/empty.", nameof(languageCode));

            languageCode = languageCode.ToLowerInvariant();

            var fileName = languageCode switch
            {
                "de" => "Localization.de.xaml",
                "en" => "Localization.en.xaml",
                _ => throw new NotSupportedException($"Language '{languageCode}' is not supported.")
            };

            var source = new Uri(
                $"pack://application:,,,/PreciousMetalsManager;component/{LocalizationFolder}{fileName}",
                UriKind.Absolute);

            var newDict = new ResourceDictionary { Source = source };

            var merged = Current.Resources.MergedDictionaries;

            var oldDict = merged.FirstOrDefault(d =>
                d.Source != null &&
                d.Source.OriginalString.Contains("/Resources/Localization/Localization.", StringComparison.OrdinalIgnoreCase));

            if (oldDict != null)
                merged.Remove(oldDict);

            merged.Add(newDict);
            CurrentLanguage = languageCode;

            LanguageStore.Save(CurrentLanguage);
        }
    }
}
