using System.Windows;

namespace PreciousMetalsManager.Services
{
    public sealed class MessageService : IMessageService
    {
        public void ShowInformation(string message, string? title = null)
            => MessageBox.Show(message, title ?? string.Empty, MessageBoxButton.OK, MessageBoxImage.Information);

        public void ShowWarning(string message, string title)
            => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);

        public void ShowError(string message, string title)
            => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);

        public bool ShowConfirmation(string message, string title)
            => MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;
    }
}