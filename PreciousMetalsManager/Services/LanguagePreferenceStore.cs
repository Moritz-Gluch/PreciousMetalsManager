using System;
using System.IO;

namespace PreciousMetalsManager.Services
{
    public sealed class LanguagePreferenceStore
    {
        private const string FileName = "language.txt";
        private readonly string _filePath;

        public LanguagePreferenceStore()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = Path.Combine(appData, "PreciousMetalsManager");
            Directory.CreateDirectory(dir);

            _filePath = Path.Combine(dir, FileName);
        }

        public string? TryLoad()
        {
            if (!File.Exists(_filePath))
                return null;

            var value = File.ReadAllText(_filePath).Trim();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        public void Save(string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
                return;

            File.WriteAllText(_filePath, languageCode.Trim());
        }
    }
}
