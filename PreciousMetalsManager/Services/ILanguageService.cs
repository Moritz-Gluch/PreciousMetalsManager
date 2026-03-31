namespace PreciousMetalsManager.Services
{
    public interface ILanguageService
    {
        string CurrentLanguage { get; }
        void ToggleLanguage();
    }
}