using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WPF_Torpedo.Models;
using WPF_Torpedo.Services;

namespace WPF_Torpedo
{
    public partial class CreateTable : Page
    {
        private Ship _selectedShip;
        private GameGrid _grid = new();
        private string draggedShip; // Húzott hajó neve
        private int draggedShipSize; // Húzott hajó mérete
        private bool isVertical = false; // Hajó orientáció (false = vízszintes, true = függőleges)
        private List<ShipPlacement> placedShips = new List<ShipPlacement>();
        private TcpClient _client;
        private IPageNavigator _navigator;

        // Hajók maximális száma
        private Dictionary<string, int> shipsCount = new Dictionary<string, int>
            {
                { "Carrier", 1 },
                { "Battleship", 1 },
                { "Submarine", 1 },
                { "Cruiser", 1 },
                { "Destroyer", 1 }
            };

        public CreateTable()
        {
            InitializeComponent();
            GenerateGrid();
        }

        public void GenerateGrid()
        {
            Viewbox viewbox = new Viewbox();

            Grid grid = new Grid
            {
                Width = 350,
                Height = 350,
                ShowGridLines = true
            };

            for (int i = 0; i < 11; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(35) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(35) });
            }

            for (int row = 0; row < 11; row++)
            {
                for (int col = 0; col < 11; col++)
                {
                    Border border = new Border
                    {
                        Background = Brushes.LightGray, // Alap háttérszín
                        BorderBrush = Brushes.Black,    // Cellahatár
                        BorderThickness = new Thickness(1)
                    };

                    border.AllowDrop = true; // Drag-and-drop engedélyezése
                    border.Drop += Grid_Drop; // Drop esemény kezelése

                    // MouseLeave event to reset color
                    border.MouseLeave += (s, e) =>
                    {
                        if (border.Background == Brushes.LightBlue)
                        {
                            border.Background = Brushes.LightGray; // Reset to original color
                        }
                    };

                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, col);
                    grid.Children.Add(border);
                }
            }
            viewbox.Child = grid;

            Grid.SetRow(viewbox, 1);
            Grid.SetColumn(viewbox, 1);
            gridMain.Children.Add(viewbox);
        }

        // Drag-and-Drop indítása
        private void Ship_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border)
            {
                draggedShip = border.Name; // Hajó neve
                draggedShipSize = int.Parse(border.Tag.ToString()); // Hajó mérete

                // Ellenőrzés: van-e még elérhető hajó az adott típusból
                if (shipsCount[draggedShip] > 0)
                {
                    DragDrop.DoDragDrop(border, draggedShip, DragDropEffects.Move);
                }
                else
                {
                    MessageBox.Show("Ezt a hajót már elhelyezted!");
                    draggedShip = null;
                }
            }
        }


        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            // Reset all placed ships
            ResetAllPlacedShips();

            // Reset all ship border backgrounds to DarkCyan
            ResetAllShipBorders();

            // Reset any other relevant variables if needed (e.g., placedShips list, etc.)
            placedShips.Clear(); // Clears the list of placed ships

            // Optionally reset other UI components, e.g., ship count, etc.
            foreach (var ship in shipsCount.Keys.ToList())
            {
                shipsCount[ship] = 1; // Resetting ship counts back to initial values (you can customize this)
            }

            MessageBox.Show("All ships have been reset.");
        }

        private void ResetAllShipBorders()
        {
            // List of all ship names you are using
            List<string> shipNames = new List<string>
                {
                    "Carrier",
                    "Battleship",
                    "Submarine",
                    "Cruiser",
                    "Destroyer"
                };

            // Reset each ship's border background to DarkCyan
            foreach (var shipName in shipNames)
            {
                ResetShipBackground(shipName); // Reset each ship's border color
            }
        }

        private void ResetAllPlacedShips()
        {
            foreach (var placedShip in placedShips.ToList())
            {
                // Remove the ship
                for (int i = 0; i < placedShip.Size; i++)
                {
                    Border targetCell = placedShip.IsVertical
                        ? GetCell(placedShip.Row + i, placedShip.Col)
                        : GetCell(placedShip.Row, placedShip.Col + i);

                    if (targetCell != null)
                    {
                        targetCell.Background = Brushes.LightGray; // Reset to default
                    }
                }

                // Remove surrounding warnings
                RemoveWarningArea(placedShip);
            }

            placedShips.Clear();
        }



        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (placedShips.Count > 0)
            {
                ShipPlacement lastPlacedShip = placedShips.Last();

                // Remove the ship's cells from the grid
                for (int i = 0; i < lastPlacedShip.Size; i++)
                {
                    Border targetCell = lastPlacedShip.IsVertical
                        ? GetCell(lastPlacedShip.Row + i, lastPlacedShip.Col)
                        : GetCell(lastPlacedShip.Row, lastPlacedShip.Col + i);

                    if (targetCell != null)
                    {
                        targetCell.Background = Brushes.LightGray; // Reset to empty cell
                    }
                }

                // Remove the warning area
                RemoveWarningArea(lastPlacedShip);

                // Remove the ship from the placedShips list
                placedShips.RemoveAt(placedShips.Count - 1);

                // Reset the ship background
                shipsCount[lastPlacedShip.ShipName]++;
                ResetShipBackground(lastPlacedShip.ShipName);

                // Redraw warning areas for all remaining ships
                foreach (var remainingShip in placedShips)
                {
                    MarkWarningArea(remainingShip.Row, remainingShip.Col, remainingShip.Size, remainingShip.IsVertical);
                }

                MessageBox.Show($"The last placed ship ({lastPlacedShip.ShipName}) has been removed.");
            }
            else
            {
                MessageBox.Show("No ships to remove.");
            }
        }


        private void RemoveWarningArea(ShipPlacement ship)
        {
            int row = ship.Row;
            int col = ship.Col;
            int size = ship.Size;
            bool isVertical = ship.IsVertical;

            for (int i = -1; i <= size; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int warningRow = isVertical ? row + i : row + j;
                    int warningCol = isVertical ? col + j : col + i;

                    // Ensure the warning area is within bounds
                    if (warningRow >= 0 && warningRow < 10 && warningCol >= 0 && warningCol < 10)
                    {
                        Border warningCell = GetCell(warningRow, warningCol);
                        if (warningCell != null)
                        {
                            // Always reset the background to default (LightGray in this case)
                            warningCell.Background = Brushes.LightGray;
                        }
                    }
                }
            }
        }

        // Hajó elhelyezése a táblán
        private void Grid_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                string shipName = e.Data.GetData(DataFormats.StringFormat).ToString();

                if (sender is Border cell)
                {
                    int row = Grid.GetRow(cell);
                    int col = Grid.GetColumn(cell);

                    // Check if the ship can be placed
                    if (CanPlaceShip(row, col))
                    {
                        // Place the ship on the grid
                        for (int i = 0; i < draggedShipSize; i++)
                        {
                            Border targetCell = isVertical
                                ? GetCell(row + i, col) // Vertical
                                : GetCell(row, col + i); // Horizontal

                            if (targetCell != null)
                            {
                                targetCell.Background = Brushes.DarkGray; // Ship's color when placed
                            }
                        }

                        // Mark surrounding tiles as warning area
                        MarkWarningArea(row, col, draggedShipSize, isVertical);

                        // Decrease the ship count
                        shipsCount[shipName]--;
                        MessageBox.Show($"{shipName} successfully placed!");

                        // Add the ship's details to the placedShips list
                        placedShips.Add(new ShipPlacement(shipName, row, col, draggedShipSize, isVertical));

                        // If the ship has been placed, change its background to red
                        if (shipsCount[shipName] == 0)
                        {
                            Border shipBorder = this.FindName(shipName) as Border;
                            shipBorder.Background = Brushes.Red; // Ship background becomes red when placed
                        }

                        Position offset = new Position { X = (sbyte)row, Y = (sbyte)col };
                        switch (shipName)
                        {
                            case "Carrier":
                                _selectedShip = new Carrier(offset);
                                break;
                            case "Battleship":
                                _selectedShip = new BattleShip(offset);
                                break;
                            case "Submarine":
                                _selectedShip = new Submarine(offset);
                                break;
                            case "Cruiser":
                                _selectedShip = new Cruiser(offset);
                                break;
                            case "Destroyer":
                                _selectedShip = new Destroyer(offset);
                                break;
                        }
                        _grid.PlaceShip(_selectedShip);
                    }
                    else
                    {
                        MessageBox.Show("Cannot place the ship here!");
                    }
                }
            }
        }



        private void ResetShipBackground(string shipName)
        {
            // Find the ship border by name and reset its background
            Border shipBorder = this.FindName(shipName) as Border;
            if (shipBorder != null)
            {
                shipBorder.Background = Brushes.DarkCyan; // Reset to DarkCyan color
            }
        }

        // Ellenőrzés, hogy a hajó elhelyezhető-e
        private bool CanPlaceShip(int row, int col)
        {
            // Check if the ship fits within the grid
            if (isVertical)
            {
                if (row + draggedShipSize > 10) return false;
            }
            else
            {
                if (col + draggedShipSize > 10) return false;
            }

            // Check for collisions and adjacency
            for (int i = 0; i < draggedShipSize; i++)
            {
                int shipRow = isVertical ? row + i : row;
                int shipCol = isVertical ? col : col + i;

                // Ensure we are within bounds
                if (shipRow >= 0 && shipRow < 10 && shipCol >= 0 && shipCol < 10)
                {
                    Border targetCell = GetCell(shipRow, shipCol);
                    if (targetCell != null && targetCell.Background == Brushes.DarkGray)
                    {
                        return false; // Collision with an existing ship
                    }

                    // Check surrounding cells for adjacency
                    for (int offsetRow = -1; offsetRow <= 1; offsetRow++)
                    {
                        for (int offsetCol = -1; offsetCol <= 1; offsetCol++)
                        {
                            int checkRow = shipRow + offsetRow;
                            int checkCol = shipCol + offsetCol;

                            if (checkRow >= 0 && checkRow < 10 && checkCol >= 0 && checkCol < 10)
                            {
                                Border adjacentCell = GetCell(checkRow, checkCol);
                                if (adjacentCell != null && adjacentCell.Background == Brushes.DarkGray)
                                {
                                    return false; // Adjacent to another ship
                                }
                            }
                        }
                    }
                }
                else
                {
                    return false; // Out of bounds
                }
            }

            return true; // No collisions or adjacency issues, placement is valid
        }
        private void MarkWarningArea(int row, int col, int size, bool isVertical)
        {
            for (int i = -1; i <= size; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int warningRow = isVertical ? row + i : row + j;
                    int warningCol = isVertical ? col + j : col + i;

                    // Ensure the warning area is within bounds
                    if (warningRow >= 0 && warningRow < 10 && warningCol >= 0 && warningCol < 10)
                    {
                        Border warningCell = GetCell(warningRow, warningCol);
                        if (warningCell != null && warningCell.Background == Brushes.LightGray) // Only mark empty cells
                        {
                            warningCell.Background = Brushes.LightSalmon; // Warning area color
                        }
                    }
                }
            }
        }

        // Cellák elérése a gridMain belső rácsból
        private Border GetCell(int row, int col)
        {
            return gridMain.Children.OfType<Viewbox>()
                .SelectMany(viewbox => ((Grid)viewbox.Child).Children.OfType<Border>())
                .FirstOrDefault(el => Grid.GetRow(el) == row && Grid.GetColumn(el) == col);
        }

        // Hajó orientáció váltása (pl. billentyűzetről)
        private void Page_Keydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.R) // R gomb megnyomásával váltás
            {
                isVertical = !isVertical;
                lblOrientation.Content = isVertical ? "Függőleges" : "Vízszintes";
            }
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.Focus(); // Ensure the page receives keyboard events
        }

        private void btnDone_Click(object sender, RoutedEventArgs e)
        {
            // Check if ships have been placed
            if (placedShips.Count == 5)
            {
                // All ships placed, navigate to the gameplay page
                var gameplayPage = new Gameplay(_navigator, _client, placedShips);
                NavigationService.Navigate(gameplayPage);
            }
            else
            {
                // If not all ships are placed, show a message
                MessageBox.Show("Please place all ships before proceeding.");
            }
        }
    }
}