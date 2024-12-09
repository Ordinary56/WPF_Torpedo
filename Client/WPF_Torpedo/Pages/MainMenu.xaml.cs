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
using WPF_Torpedo.Models;
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
        private StateManager _stateManager;
        private readonly Player _player;
        public MainMenu(IPageNavigator navigator, StateManager manager, Player player)
        {
            InitializeComponent();
            _navigator = navigator;
            _stateManager = manager;
            _player = player;
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
            if (txtPlayerName.Text.Length == 0)
            {
                MessageBox.Show("Kérlek adj meg egy felhasználónevet");
                return;
            }
            _player.Username = txtPlayerName.Text;
            _player.ConnectToServer();
            byte[] buffer = new byte[1024];
            _player.Stream.Read(buffer, 0, buffer.Length);
            string data = Encoding.UTF8.GetString(buffer);
            while(data == "1")
            {
                buffer = new byte[1024];
                _player.Stream.Read(buffer, 0, buffer.Length);
                data = Encoding.UTF8.GetString(buffer);
            }


            _navigator.MoveToPage<CreateTable>();
        }

        private void ChangeState(object sender, RoutedEventArgs e)
        {
            if(sender is Button btn)
            {
                _stateManager.SelectedGame = btn.Tag.ToString() == "0" ? GAME_STATE.SINGLE : btn.Tag.ToString() == "1" ? GAME_STATE.MULTI : null;
            }
        }
    }
}
