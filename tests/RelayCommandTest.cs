using Microsoft.VisualStudio.TestTools.UnitTesting;
using PreciousMetalsManager.Models;
using System;
using System.Windows.Input;
using System.Windows.Threading;

namespace PreciousMetalsManager.Tests
{
    [TestClass]
    public class RelayCommandTest
    {
        private const string ExpectedParameter = "test-parameter";
        private const int ExpectedPredicateParameter = 42;

        [TestMethod]
        public void Constructor_WithNullExecute_ThrowsArgumentNullException()
        {
            AssertThrows<ArgumentNullException>(() => _ = new RelayCommand(null!));
        }

        [TestMethod]
        public void CanExecute_WithoutPredicate_ReturnsTrue()
        {
            var command = new RelayCommand(_ => { });

            var result = command.CanExecute(null);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanExecute_WithPredicateReturningTrue_ReturnsTrue()
        {
            var command = new RelayCommand(_ => { }, _ => true);

            var result = command.CanExecute(null);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanExecute_WithPredicateReturningFalse_ReturnsFalse()
        {
            var command = new RelayCommand(_ => { }, _ => false);

            var result = command.CanExecute(null);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Execute_InvokesExecuteDelegate()
        {
            var wasCalled = false;
            var command = new RelayCommand(_ => wasCalled = true);

            command.Execute(null);

            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        public void Execute_PassesParameterToExecuteDelegate()
        {
            object? receivedParameter = null;
            var command = new RelayCommand(parameter => receivedParameter = parameter);

            command.Execute(ExpectedParameter);

            Assert.AreEqual(ExpectedParameter, receivedParameter);
        }

        [TestMethod]
        public void CanExecute_PassesParameterToPredicate()
        {
            object? receivedParameter = null;

            var command = new RelayCommand(
                _ => { },
                parameter =>
                {
                    receivedParameter = parameter;
                    return true;
                });

            _ = command.CanExecute(ExpectedPredicateParameter);

            Assert.AreEqual(ExpectedPredicateParameter, receivedParameter);
        }

        [TestMethod]
        public void CanExecuteChanged_WhenSubscribed_IsRaisedByCommandManagerRequerySuggested()
        {
            var command = new RelayCommand(_ => { });
            var eventRaised = false;
            EventHandler handler = (_, _) => eventRaised = true;

            try
            {
                command.CanExecuteChanged += handler;

                CommandManager.InvalidateRequerySuggested();
                DrainDispatcher();

                Assert.IsTrue(eventRaised);
            }
            finally
            {
                command.CanExecuteChanged -= handler;
            }
        }

        private static void DrainDispatcher()
        {
            var frame = new DispatcherFrame();

            Dispatcher.CurrentDispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() => frame.Continue = false));

            Dispatcher.PushFrame(frame);
        }

        private static void AssertThrows<TException>(Action action) where TException : Exception
        {
            try
            {
                action();
                Assert.Fail($"Expected exception {typeof(TException).Name} was not thrown.");
            }
            catch (TException)
            {
                // expected
            }
        }
    }
}
