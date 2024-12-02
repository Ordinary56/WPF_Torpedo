using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPF_Torpedo.Services;

namespace WPF_Torpedo.Pages
{
    /// <summary>
    /// Interaction logic for MainMenu.xaml
    /// </summary>
    public partial class MainMenu : Page
    {
        private IPageNavigator _navigator;
        public MainMenu(IPageNavigator navigator)
        {
            InitializeComponent();
            _navigator = navigator;
        }

        private void btnStartGame_Click(object sender, RoutedEventArgs e)
        {
            _navigator.MoveToPage<CreateTable>();
        }
    }
}
