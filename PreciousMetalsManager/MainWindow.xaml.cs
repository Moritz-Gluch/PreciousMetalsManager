using PreciousMetalsManager.ViewModels;
using PreciousMetalsManager.Views;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PreciousMetalsManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ViewModel();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddHoldingWindow();
            if (addWindow.ShowDialog() == true)
            {
                if (DataContext is ViewModel vm)
                {
                    vm.Holdings.Add(addWindow.NewHolding);
                }
            }
        }
    }
}
