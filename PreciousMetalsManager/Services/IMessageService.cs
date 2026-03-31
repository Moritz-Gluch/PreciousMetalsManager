namespace PreciousMetalsManager.Services
{
    public interface IMessageService
    {
        void ShowInformation(string message, string? title = null);
        void ShowWarning(string message, string title);
        void ShowError(string message, string title);
        bool ShowConfirmation(string message, string title);
    }
}