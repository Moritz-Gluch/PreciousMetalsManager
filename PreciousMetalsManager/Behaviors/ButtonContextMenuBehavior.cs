using System.Windows;
using System.Windows.Controls;

namespace PreciousMetalsManager.Behaviors
{
    public static class ButtonContextMenuBehavior
    {
        public static readonly DependencyProperty OpenOnClickProperty =
            DependencyProperty.RegisterAttached(
                "OpenOnClick",
                typeof(bool),
                typeof(ButtonContextMenuBehavior),
                new PropertyMetadata(false, OnOpenOnClickChanged));

        public static void SetOpenOnClick(DependencyObject element, bool value)
            => element.SetValue(OpenOnClickProperty, value);

        public static bool GetOpenOnClick(DependencyObject element)
            => (bool)element.GetValue(OpenOnClickProperty);

        private static void OnOpenOnClickChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Button button)
                return;

            button.Click -= Button_Click;

            if (e.NewValue is true)
                button.Click += Button_Click;
        }

        private static void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.ContextMenu == null)
                return;

            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.IsOpen = true;
            e.Handled = true;
        }
    }
}