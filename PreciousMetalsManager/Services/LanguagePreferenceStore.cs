using System;
using System.IO;

namespace PreciousMetalsManager.Services
{
    public sealed class LanguagePreferenceStore
    {
        private const string FileName = "language.txt";
        private readonly string _filePath;

        public LanguagePreferenceStore()
            : this(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "PreciousMetalsManager"))
        {
        }

        public LanguagePreferenceStore(string baseDirectory)
        {
            if (string.IsNullOrWhiteSpace(baseDirectory))
                throw new ArgumentException("baseDirectory must not be null/empty.", nameof(baseDirectory));

            Directory.CreateDirectory(baseDirectory);
            _filePath = Path.Combine(baseDirectory, FileName);
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
