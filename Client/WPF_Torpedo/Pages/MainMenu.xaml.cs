using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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
        private TcpClient client;
        private IPageNavigator _navigator;
        public MainMenu(IPageNavigator navigator)
        {
            InitializeComponent();
            _navigator = navigator;
        }

        private void ConnectToServer()
        {
            try
            {
                client = new TcpClient();
                client.Connect("127.0.0.1", 37065);
                MessageBox.Show("Successfully connected to the server!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to connect to the server: {ex.Message}");
            }
        }

        private void btnStartGame_Click(object sender, RoutedEventArgs e)
        {
            ConnectToServer();
            _navigator.MoveToPage<CreateTable>();
        }



    }
}
