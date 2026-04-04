using Microsoft.VisualStudio.TestTools.UnitTesting;
using PreciousMetalsManager.Behaviors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PreciousMetalsManager.Tests
{
    [STATestClass]
    public sealed class BehaviorTest
    {
        private const string FirstItem = "Gold";
        private const string SecondItem = "Silver";
        private const string ThirdItem = "Platinum";

        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            EnsureWpfApp();
        }

        [TestMethod]
        public void ButtonContextMenuBehavior_SetAndGetOpenOnClick_WorksCorrectly()
        {
            var button = new Button();

            ButtonContextMenuBehavior.SetOpenOnClick(button, true);

            Assert.IsTrue(ButtonContextMenuBehavior.GetOpenOnClick(button));

            ButtonContextMenuBehavior.SetOpenOnClick(button, false);

            Assert.IsFalse(ButtonContextMenuBehavior.GetOpenOnClick(button));
        }

        [TestMethod]
        public void ButtonContextMenuBehavior_Click_WithContextMenu_OpensMenu_AndMarksEventHandled()
        {
            var button = new Button
            {
                ContextMenu = new ContextMenu()
            };

            using var host = CreateHostWindow(button);

            ButtonContextMenuBehavior.SetOpenOnClick(button, true);

            var args = new RoutedEventArgs(Button.ClickEvent, button);
            button.RaiseEvent(args);

            Assert.IsTrue(button.ContextMenu.IsOpen);
            Assert.AreSame(button, button.ContextMenu.PlacementTarget);
            Assert.IsTrue(args.Handled);
        }

        [TestMethod]
        public void ButtonContextMenuBehavior_Click_WithoutContextMenu_DoesNothing()
        {
            var button = new Button();

            using var host = CreateHostWindow(button);

            ButtonContextMenuBehavior.SetOpenOnClick(button, true);

            var args = new RoutedEventArgs(Button.ClickEvent, button);
            button.RaiseEvent(args);

            Assert.IsFalse(args.Handled);
        }

        [TestMethod]
        public void ButtonContextMenuBehavior_WhenDisabled_Click_DoesNotOpenContextMenu()
        {
            var button = new Button
            {
                ContextMenu = new ContextMenu()
            };

            using var host = CreateHostWindow(button);

            ButtonContextMenuBehavior.SetOpenOnClick(button, true);
            ButtonContextMenuBehavior.SetOpenOnClick(button, false);

            var args = new RoutedEventArgs(Button.ClickEvent, button);
            button.RaiseEvent(args);

            Assert.IsFalse(button.ContextMenu.IsOpen);
            Assert.IsFalse(args.Handled);
        }

        [TestMethod]
        public void DataGridSelectionBehavior_SetAndGetSelectionChangedCommand_WorksCorrectly()
        {
            var dataGrid = new DataGrid();
            var command = new RecordingCommand();

            DataGridSelectionBehavior.SetSelectionChangedCommand(dataGrid, command);

            Assert.AreSame(command, DataGridSelectionBehavior.GetSelectionChangedCommand(dataGrid));

            DataGridSelectionBehavior.SetSelectionChangedCommand(dataGrid, null);

            Assert.IsNull(DataGridSelectionBehavior.GetSelectionChangedCommand(dataGrid));
        }

        [TestMethod]
        public void DataGridSelectionBehavior_WhenSelectionChanges_ExecutesCommandWithSelectedItems()
        {
            var items = new List<string> { FirstItem, SecondItem };
            var command = new RecordingCommand();
            var dataGrid = CreateDataGrid(items);

            using var host = CreateHostWindow(dataGrid);

            DataGridSelectionBehavior.SetSelectionChangedCommand(dataGrid, command);

            dataGrid.SelectedItem = FirstItem;

            Assert.AreEqual(1, command.ExecuteCallCount);
            Assert.IsNotNull(command.LastParameter);

            var selection = command.LastParameter as IList;
            Assert.IsNotNull(selection);
            Assert.HasCount(1, selection);
            Assert.AreEqual(FirstItem, selection[0]);
        }

        [TestMethod]
        public void DataGridSelectionBehavior_WhenSelectionIsCleared_ExecutesCommandWithEmptySelection()
        {
            var items = new List<string> { FirstItem, SecondItem };
            var command = new RecordingCommand();
            var dataGrid = CreateDataGrid(items);

            using var host = CreateHostWindow(dataGrid);

            DataGridSelectionBehavior.SetSelectionChangedCommand(dataGrid, command);

            dataGrid.SelectedItem = items[0];
            Assert.AreEqual(1, command.ExecuteCallCount);

            dataGrid.SelectedItem = null;

            Assert.AreEqual(2, command.ExecuteCallCount);

            var selection = command.LastParameter as IList;
            Assert.IsNotNull(selection);
            Assert.IsEmpty(selection);
        }

        [TestMethod]
        public void DataGridSelectionBehavior_WhenMultipleItemsAreSelected_ExecutesCommandWithAllSelectedItems()
        {
            var items = new List<string> { FirstItem, SecondItem, ThirdItem };
            var command = new RecordingCommand();
            var dataGrid = CreateDataGrid(items);
            dataGrid.SelectionMode = DataGridSelectionMode.Extended;

            using var host = CreateHostWindow(dataGrid);

            DataGridSelectionBehavior.SetSelectionChangedCommand(dataGrid, command);

            dataGrid.SelectedItems.Add(items[0]);
            dataGrid.SelectedItems.Add(items[1]);

            Assert.AreEqual(2, command.ExecuteCallCount);

            var selection = command.LastParameter as IList;
            Assert.IsNotNull(selection);
            Assert.HasCount(2, selection);
            CollectionAssert.AreEquivalent(
                new[] { FirstItem, SecondItem },
                selection.Cast<string>().ToArray());
        }

        [TestMethod]
        public void DataGridSelectionBehavior_WhenCanExecuteIsFalse_DoesNotExecuteCommand()
        {
            var items = new List<string> { FirstItem, SecondItem };
            var command = new RecordingCommand { CanExecuteResult = false };
            var dataGrid = CreateDataGrid(items);

            using var host = CreateHostWindow(dataGrid);

            DataGridSelectionBehavior.SetSelectionChangedCommand(dataGrid, command);

            dataGrid.SelectedItem = items[0];

            Assert.AreEqual(1, command.CanExecuteCallCount);
            Assert.AreEqual(0, command.ExecuteCallCount);
        }

        [TestMethod]
        public void DataGridSelectionBehavior_WhenCommandRemoved_DoesNotExecuteOldCommand()
        {
            var items = new List<string> { FirstItem, SecondItem };
            var command = new RecordingCommand();
            var dataGrid = CreateDataGrid(items);

            using var host = CreateHostWindow(dataGrid);

            DataGridSelectionBehavior.SetSelectionChangedCommand(dataGrid, command);
            DataGridSelectionBehavior.SetSelectionChangedCommand(dataGrid, null);

            dataGrid.SelectedItem = items[0];

            Assert.AreEqual(0, command.ExecuteCallCount);
        }

        [TestMethod]
        public void DataGridSelectionBehavior_WhenCommandReplaced_UsesNewCommandOnly()
        {
            var items = new List<string> { FirstItem, SecondItem };
            var firstCommand = new RecordingCommand();
            var secondCommand = new RecordingCommand();
            var dataGrid = CreateDataGrid(items);

            using var host = CreateHostWindow(dataGrid);

            DataGridSelectionBehavior.SetSelectionChangedCommand(dataGrid, firstCommand);
            DataGridSelectionBehavior.SetSelectionChangedCommand(dataGrid, secondCommand);

            dataGrid.SelectedItem = items[0];

            Assert.AreEqual(0, firstCommand.ExecuteCallCount);
            Assert.AreEqual(1, secondCommand.ExecuteCallCount);
        }

        [TestMethod]
        public void DataGridSelectionBehavior_WhenSameCommandIsSetTwice_ExecutesOnlyOnce()
        {
            var items = new List<string> { FirstItem, SecondItem };
            var command = new RecordingCommand();
            var dataGrid = CreateDataGrid(items);

            using var host = CreateHostWindow(dataGrid);

            DataGridSelectionBehavior.SetSelectionChangedCommand(dataGrid, command);
            DataGridSelectionBehavior.SetSelectionChangedCommand(dataGrid, command);

            dataGrid.SelectedItem = items[0];

            Assert.AreEqual(1, command.ExecuteCallCount);
        }

        [TestMethod]
        public void ButtonContextMenuBehavior_OnNonButton_DoesNotThrow()
        {
            var border = new Border();

            ButtonContextMenuBehavior.SetOpenOnClick(border, true);

            Assert.IsTrue(ButtonContextMenuBehavior.GetOpenOnClick(border));
        }

        private static DataGrid CreateDataGrid(IEnumerable<string> items)
        {
            var dataGrid = new DataGrid
            {
                AutoGenerateColumns = true,
                SelectionMode = DataGridSelectionMode.Single,
                CanUserAddRows = false,
                ItemsSource = items.ToList()
            };

            return dataGrid;
        }

        private static WindowHost CreateHostWindow(FrameworkElement content)
        {
            var window = new Window
            {
                Width = 300,
                Height = 200,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None,
                Content = content
            };

            window.Show();
            content.UpdateLayout();

            return new WindowHost(window);
        }

        private static void EnsureWpfApp()
        {
            if (Application.Current is not null)
                return;

            _ = new Application
            {
                ShutdownMode = ShutdownMode.OnExplicitShutdown
            };
        }

        private sealed class RecordingCommand : ICommand
        {
            public bool CanExecuteResult { get; set; } = true;
            public int CanExecuteCallCount { get; private set; }
            public int ExecuteCallCount { get; private set; }
            public object? LastParameter { get; private set; }

            public bool CanExecute(object? parameter)
            {
                CanExecuteCallCount++;
                LastParameter = parameter;
                return CanExecuteResult;
            }

            public void Execute(object? parameter)
            {
                ExecuteCallCount++;
                LastParameter = parameter;
            }

            public event EventHandler? CanExecuteChanged;
        }

        private sealed class WindowHost : IDisposable
        {
            private readonly Window _window;

            public WindowHost(Window window)
            {
                _window = window;
            }

            public void Dispose()
            {
                if (_window.IsVisible)
                    _window.Close();
            }
        }
    }
}
