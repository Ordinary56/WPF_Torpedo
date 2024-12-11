using System;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using WPF_Torpedo.Helpers;
using WPF_Torpedo.Models;
using WPF_Torpedo.Services;

namespace WPF_Torpedo.Pages
{
    /// <summary>
    /// Interaction logic for Gameplay.xaml
    /// </summary>
    public partial class Gameplay : Page
    {
        private Position selectedPosition = new Position { X = -1, Y = -1 };
        private bool isSelected = false;
        private IPageNavigator _navigator;
        private TcpClient _client;
        private NetworkStream _stream;
        private bool isPlayerTurn = true; // Tracks whose turn it is
        List<ShipPlacement> placedShips = new List<ShipPlacement>();
        Player _player;
        GameGrid _eGrid;


        public Gameplay(IPageNavigator navigator,  List<ShipPlacement> placedShips, Player player)
        {
            InitializeComponent();
            this.placedShips = placedShips;
            _navigator = navigator;
            _player = player;
            _player.SendInfoToServer();
            StartListeningForMessages();

            // Generate the grid with ships already placed
            GenerateGridForGameplay();

            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                parentWindow.Closed += OnParentWindowClosed;
            }
        }

        private void OnParentWindowClosed(object sender, EventArgs e)
        {
            try
            {
                _player.Stream.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during cleanup: {ex.Message}");
            }
        }


        private void GenerateGridForGameplay()
        {
            // Create the player's grid
            Grid playerGrid = new Grid
            {
                Width = 350,
                Height = 350,
                ShowGridLines = true
            };

            for (int i = 0; i < 11; i++)
            {
                playerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(35) });
                playerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(35) });
            }

            for (int row = 0; row < 11; row++)
            {
                for (int col = 0; col < 11; col++)
                {
                    Border border = new Border
                    {
                        Background = Brushes.LightGray, // Default background color for empty cells
                        BorderBrush = Brushes.Black,    // Cell border
                        BorderThickness = new Thickness(1)
                    };

                    // MouseLeave event to reset color
                    border.MouseLeave += (s, e) =>
                    {
                        if (border.Background == Brushes.LightBlue)
                        {
                            border.Background = Brushes.LightGray; // Reset color when mouse leaves
                        }
                    };

                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, col);
                    playerGrid.Children.Add(border);
                }
            }

            // Add the player's grid to the gridMain container
            Grid.SetRow(playerGrid, 2);  // Position under "Te" label (row 2)
            Grid.SetColumn(playerGrid, 0); // Position in the first column (player's side)
            gridMain.Children.Add(playerGrid);

            // Create the enemy's grid (similar to the player's grid)
            Grid enemyGrid = new Grid
            {
                Width = 350,
                Height = 350,
                ShowGridLines = true
            };

            for (int i = 0; i < 11; i++)
            {
                enemyGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(35) });
                enemyGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(35) });
            }

            for (int row = 0; row < 11; row++)
            {
                for (int col = 0; col < 11; col++)
                {
                    Border border = new Border
                    {
                        Background = Brushes.LightGray, // Default background color for empty cells
                        BorderBrush = Brushes.Black,    // Cell border
                        BorderThickness = new Thickness(1),
                        Tag = $"{row},{col}"
                    };

                    // MouseLeave event to reset color
                    border.MouseLeave += (s, e) =>
                    {
                        if (border.Background == Brushes.LightBlue)
                        {
                            border.Background = Brushes.LightGray; // Reset color when mouse leaves
                        }
                    };
                    border.MouseLeftButtonDown += (s, e) =>
                    {
                        if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed &&
                        isPlayerTurn && s is Border bord)
                        {
                            var coords = bord.Tag.ToString().Split(',');
                            Position positions = new() { X = Convert.ToSByte(coords[0]), Y = Convert.ToSByte(coords[^1]) };
                            SendAttackCoordinates(positions);
                            isPlayerTurn = false;
                        }
                    };

                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, col);
                    enemyGrid.Children.Add(border);
                }
            }

            // Add the enemy's grid to the gridMain container
            Grid.SetRow(enemyGrid, 2);  // Position under "Ellenfél" label (row 2)
            Grid.SetColumn(enemyGrid, 1); // Position in the second column (opponent's side)
            gridMain.Children.Add(enemyGrid);

            // Now place the ships on the grids
            PlaceShipsOnGrid(playerGrid); // Place player ships
        }


        // Method to place ships on the grid
        private void PlaceShipsOnGrid(Grid grid)
        {
            foreach (var placedShip in placedShips)
            {
                for (int i = 0; i < placedShip.Size; i++)
                {
                    // Find the appropriate border cell based on ship position
                    Border targetCell = placedShip.IsVertical
                        ? GetGridCell(placedShip.Row + i, placedShip.Col, grid)
                        : GetGridCell(placedShip.Row, placedShip.Col + i, grid);

                    if (targetCell != null)
                    {
                        targetCell.Background = Brushes.DarkGray; // Set color for ships
                    }
                }
            }
        }

        private void NotifyServer(string message)
        {
            SendMessage(message);
        }

        private void SendMessage(string message)
        {
            try
            {
                if (_stream != null && _client.Connected)
                {
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    _stream.Write(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending message: {ex.Message}");
            }
        }

        private void SendAttackCoordinates(Position position)
        {
            _player.SendFireTo(position.X, position.Y);
        }

        private async void StartListeningForMessages()
        {
            try
            {
                byte[] buffer = new byte[1024];

                while (_player.Stream != null)
                {
                    int bytesRead = await _player.Stream.ReadAsync(buffer);
                    if (bytesRead > 0)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        HandleServerResponse(message);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error receiving messages: {ex.Message}");
            }
        }

        private void HandleServerResponse(string response)
        {
            // Handle server response for hit/miss or game end

            string[] parts = response.Split(',');
            string result;
            int X = 0, Y = 0;
            if (parts[0] == "Info")
            {
                _eGrid = GridParser.ParseGrid(parts[^1]);
            }
            else if (parts[0] == "Attack")
            {
                _player.RecieveFire(int.Parse(parts[1]), int.Parse(parts[^1]));
                if (_player.Grid.GetCell(int.Parse(parts[1]), int.Parse(parts[^1])) > 0)
                {
                    result = "Hit";
                }
                else result = "Miss";
                UpdateGridCell(X, Y, result, playerGrid);
            }
        }


        private void UpdateGridCell(int x, int y, string result, Grid chosenGrid)
        {
            // Find the corresponding grid cell and color it based on result (Hit or Miss)
            Border cell = GetGridCell(x, y, chosenGrid); // Use correct grid (gridOpponent for opponent)
            if (cell != null)
            {
                if (result == "Hit")
                {
                    cell.Background = Brushes.Red; // Color for a hit
                }
                else if (result == "Miss")
                {
                    cell.Background = Brushes.Blue; // Color for a miss
                }
            }
        }

        // Helper method to get the cell from the grid (updated)
        private Border GetGridCell(int row, int col, Grid grid)
        {
            foreach (var child in grid.Children)
            {
                if (child is Border border)
                {
                    int r = Grid.GetRow(border);
                    int c = Grid.GetColumn(border);

                    if (r == row && c == col)
                    {
                        return border;
                    }
                }
            }
            return null; // Return null if no matching cell is found
        }
    }
}
