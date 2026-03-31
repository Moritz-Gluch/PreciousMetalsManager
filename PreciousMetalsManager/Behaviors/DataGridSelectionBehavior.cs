using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PreciousMetalsManager.Behaviors
{
    public static class DataGridSelectionBehavior
    {
        public static readonly DependencyProperty SelectionChangedCommandProperty =
            DependencyProperty.RegisterAttached(
                "SelectionChangedCommand",
                typeof(ICommand),
                typeof(DataGridSelectionBehavior),
                new PropertyMetadata(null, OnSelectionChangedCommandChanged));

        public static void SetSelectionChangedCommand(DependencyObject element, ICommand? value)
            => element.SetValue(SelectionChangedCommandProperty, value);

        public static ICommand? GetSelectionChangedCommand(DependencyObject element)
            => (ICommand?)element.GetValue(SelectionChangedCommandProperty);

        private static void OnSelectionChangedCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DataGrid dataGrid)
                return;

            dataGrid.SelectionChanged -= DataGrid_SelectionChanged;

            if (e.NewValue is ICommand)
                dataGrid.SelectionChanged += DataGrid_SelectionChanged;
        }

        private static void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not DataGrid dataGrid)
                return;

            var command = GetSelectionChangedCommand(dataGrid);
            if (command == null)
                return;

            var selection = dataGrid.SelectedItems.Cast<object>().ToList();

            if (command.CanExecute(selection))
                command.Execute(selection);
        }
    }
}