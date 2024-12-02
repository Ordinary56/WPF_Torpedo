using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WPF_Torpedo
{
    public partial class CreateTable : Page
    {
        private string draggedShip; // Húzott hajó neve
        private int draggedShipSize; // Húzott hajó mérete
        private bool isVertical = false; // Hajó orientáció (false = vízszintes, true = függőleges)

        // Hajók maximális száma
        private Dictionary<string, int> shipsCount = new Dictionary<string, int>
        {
            { "AircraftCarrier", 1 },
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

                    // MouseEnter event to show preview
                    border.MouseEnter += (s, e) =>
                    {
                        if (draggedShip != null)
                        {
                            // Get the current row and column of the border (cell)
                            int currentRow = Grid.GetRow(border);
                            int currentCol = Grid.GetColumn(border);

                            // Check if the ship can be placed at this position
                            if (CanPlaceShip(currentRow, currentCol))
                            {
                                border.Background = Brushes.LightBlue; // Show preview color if it can be placed
                            }
                        }
                    };

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

                    // Ellenőrzés, hogy elfér-e a hajó a kiválasztott pozícióban
                    if (CanPlaceShip(row, col))
                    {
                        // Hajó elhelyezése
                        for (int i = 0; i < draggedShipSize; i++)
                        {
                            Border targetCell = isVertical
                                ? GetCell(row + i, col) // Fü ggőleges
                                : GetCell(row, col + i); // Vízszintes

                            if (targetCell != null)
                            {
                                targetCell.Background = Brushes.DarkGray; // Hajó színe
                            }
                        }

                        // Hajók számának csökkentése
                        shipsCount[shipName]--;
                        MessageBox.Show($"{shipName} sikeresen elhelyezve!");

                        if (shipsCount[shipName] == 0)
                        {
                            Border shipBorder = this.FindName(shipName) as Border;
                            SolidColorBrush solidColorBrush = new SolidColorBrush(Color.FromArgb(Colors.Red.A, Colors.Red.R, Colors.Red.G, Colors.Red.B));
                            shipBorder.Background = solidColorBrush;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Nem lehet elhelyezni a hajót itt!");
                    }
                }
            }
        }

        // Ellenőrzés, hogy a hajó elhelyezhető-e
        private bool CanPlaceShip(int row, int col)
        {
            // Ellenőrzés, hogy a hajó nem lóg ki a rácsból
            if (isVertical)
            {
                if (row + draggedShipSize > 10) return false;
            }
            else
            {
                if (col + draggedShipSize > 10) return false;
            }

            // Ellenőrzés, hogy a hajó nem ütközik más hajóval, vagy nem érintkezik velük
            for (int i = 0; i < draggedShipSize; i++)
            {
                int checkRow = isVertical ? row + i : row;
                int checkCol = isVertical ? col : col + i;

                // Ensure we are within bounds
                if (checkRow >= 0 && checkRow < 10 && checkCol >= 0 && checkCol < 10)
                {
                    Border targetCell = GetCell(checkRow, checkCol);
                    if (targetCell != null && targetCell.Background == Brushes.DarkGray)
                    {
                        return false; // Ütközés vagy érintkezés más hajóval
                    }
                }
                else
                {
                    return false; // Out of bounds
                }
            }
            return true; // No collisions, valid placement
        }

        // Cellák elérése a gridMain belső rácsból
        private Border GetCell(int row, int col)
        {
            return gridMain.Children.OfType<Viewbox>()
                .SelectMany(viewbox => ((Grid)viewbox.Child).Children.OfType<Border>())
                .FirstOrDefault(el => Grid.GetRow(el) == row && Grid.GetColumn(el) == col);
        }

        // Hajó orientáció váltása (pl. billentyűzetről)
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.R) // R gomb megnyomásával váltás
            {
                isVertical = !isVertical;
                lblOrientation.Content = isVertical ? "Függőleges" : "Vízszintes";
            }
        }
    }
}