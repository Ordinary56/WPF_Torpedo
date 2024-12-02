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
using WPF_Torpedo.Pages;
using WPF_Torpedo.Services;

namespace WPF_Torpedo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IPageNavigator _navigator;
        public IPageNavigator Navigator => _navigator;
        public MainWindow(IPageNavigator navigator)
        {
            InitializeComponent();
            _navigator = navigator;
            DataContext = this;
            _navigator.CurrentPage = new MainMenu();
        }
    }
}