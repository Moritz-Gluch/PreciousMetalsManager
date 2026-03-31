namespace PreciousMetalsManager.Services
{
    public interface IFileDialogService
    {
        string? ShowOpenFileDialog(string filter, string title);
        string? ShowSaveFileDialog(string filter, string title, string fileName);
    }
}