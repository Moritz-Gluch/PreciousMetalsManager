using System.Windows;

namespace PreciousMetalsManager.Services
{
    public sealed class TextProvider : ITextProvider
    {
        public string GetString(string key)
            => Application.Current?.TryFindResource(key) as string ?? key;
    }
}