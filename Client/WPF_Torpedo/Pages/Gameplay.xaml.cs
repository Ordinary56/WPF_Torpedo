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
using System.Windows.Shapes;
using WPF_Torpedo.Models;
using WPF_Torpedo.Services;

namespace WPF_Torpedo
{
    /// <summary>
    /// Interaction logic for Gameplay.xaml
    /// </summary>
    public partial class Gameplay : Page
    {
        private Position selectedPosition = new Position { X = -1, Y = -1};
        private bool isSelected = false;
        private IPageNavigator _navigator;
        public Gameplay(IPageNavigator navigator)
        {
            InitializeComponent();
            GenerateGrid(2, 0, "Player");
            GenerateGrid(2, 1, "Opponent");
            _navigator = navigator;
        }

        public void GenerateGrid(int posRow, int posCol, string tableType)
        {
            Viewbox viewbox = new Viewbox();

            Grid grid = new Grid
            {
                Width = 300,
                Height = 300,
                ShowGridLines = true
            };

            for (int i = 0; i < 11; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });
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

                    if (tableType == "Opponent")
                    {
                        border.MouseLeftButtonDown += (s, e) =>
                        {
                            Border borderEl = s as Border;

                            if (borderEl != null)
                            {
                                Position positionTemp = new Position { X = (sbyte)row, Y = (sbyte)col };
                                // Ha van már kiválasztott border
                                if (isSelected)
                                {
                                    // Ha ugyanazt a bordert próbálják újra kijelölni, megszünteti a kijelölést
                                    if (selectedPosition.X == positionTemp.X && selectedPosition.Y == positionTemp.Y)
                                    {
                                        borderEl.Background = Brushes.LightGray;
                                        selectedPosition = new Position { X = -1, Y = -1 };
                                        isSelected = false;
                                        return;
                                    }

                                    // Másik bordert választottak ki, előzőt alapállapotba állítjuk
                                    borderEl.Background = Brushes.LightGray;
                                }

                                // Új border kijelölése
                                borderEl.Background = Brushes.Blue; // Kijelölés színe
                                selectedPosition = positionTemp; // Tároljuk a pozíciót és a border-t
                                isSelected = true;
                            }
                        };
                    }

                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, col);
                    grid.Children.Add(border);
                }
            }
            viewbox.Child = grid;

            Grid.SetRow(viewbox, posRow);
            Grid.SetColumn(viewbox, posCol);
            gridMain.Children.Add(viewbox);
        }
    }
}
