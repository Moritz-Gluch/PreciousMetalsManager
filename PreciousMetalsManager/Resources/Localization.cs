using System.Globalization;

namespace PreciousMetalsManager.Resources
{
    /// <summary>
    /// Provides access to localized strings and allows switching the application language at runtime.
    /// </summary>
    public static class Localization
    {
        //public static string MainWindow_Title => Resources.MainWindow_Title;

        public static string AddButton => Resources.AddButton;
        public static string EditButton => Resources.EditButton;
        public static string DeleteButton => Resources.DeleteButton;

        /// <summary>
        /// Sets the application language at runtime.
        /// </summary>
        /// <param name="cultureName">Culture name, e.g. "de" or "en".</param>
        public static void SetLanguage(string cultureName)
        {
            var culture = new CultureInfo(cultureName);
            Resources.Culture = culture;
        }
    }
}