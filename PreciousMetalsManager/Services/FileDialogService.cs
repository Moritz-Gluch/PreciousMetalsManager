using Microsoft.Win32;

namespace PreciousMetalsManager.Services
{
    public sealed class FileDialogService : IFileDialogService
    {
        public string? ShowOpenFileDialog(string filter, string title)
        {
            var dialog = new OpenFileDialog
            {
                Filter = filter,
                Title = title
            };

            return dialog.ShowDialog() == true
                ? dialog.FileName
                : null;
        }

        public string? ShowSaveFileDialog(string filter, string title, string fileName)
        {
            var dialog = new SaveFileDialog
            {
                Filter = filter,
                Title = title,
                FileName = fileName
            };

            return dialog.ShowDialog() == true
                ? dialog.FileName
                : null;
        }
    }
}