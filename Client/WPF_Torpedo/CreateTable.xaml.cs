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

namespace WPF_Torpedo
{
    /// <summary>
    /// Interaction logic for CreateTable.xaml
    /// </summary>
    public partial class CreateTable : Window
    {
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

            List<string> strings = new List<string>(["", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J"]);

            for (int rowCounter = 0; rowCounter < 11; rowCounter++)
            {
                RowDefinition rowDef = new RowDefinition();
                rowDef.Height = new GridLength(35);

                //if (rowCounter == 0)
                //{
                //    Label label = new Label
                //    {
                //        Content = strings[rowCounter],
                //        HorizontalContentAlignment = HorizontalAlignment.Center,
                //        VerticalContentAlignment = VerticalAlignment.Center
                //    };
                //    label.Content = strings[rowCounter];
                //}

                grid.RowDefinitions.Add(rowDef);
            }

            for (int i = 0; i < 11; i++)
            {
                ColumnDefinition colDef = new ColumnDefinition();
                colDef.Width = new GridLength(35);
                grid.ColumnDefinitions.Add(colDef);
            }

            Grid.SetRow(viewbox, 1);
            Grid.SetColumn(viewbox, 1);

            viewbox.Child = grid;

            gridMain.Children.Add(viewbox);
        }
    }
}
