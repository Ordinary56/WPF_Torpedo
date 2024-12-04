using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Torpedo.Models
{

    public class ShipPlacement
    {
        public string ShipName { get; set; }
        public int Row { get; set; }
        public int Col { get; set; }
        public int Size { get; set; }
        public bool IsVertical { get; set; }

        public ShipPlacement(string shipName, int row, int col, int size, bool isVertical)
        {
            ShipName = shipName;
            Row = row;
            Col = col;
            Size = size;
            IsVertical = isVertical;
        }
    }
}
