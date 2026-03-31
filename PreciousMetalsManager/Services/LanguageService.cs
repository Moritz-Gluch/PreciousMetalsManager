namespace PreciousMetalsManager.Services
{
    public sealed class LanguageService : ILanguageService
    {
        public string CurrentLanguage => App.CurrentLanguage;

        public void ToggleLanguage()
            => App.SetLanguage(App.CurrentLanguage == "en" ? "de" : "en");
    }
}