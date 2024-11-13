using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WPF_Torpedo
{
    /// <summary>
    /// This class represents the game's row
    /// <remarks>The game's grid is 10x10 by default</remarks>
    /// </summary>
    public class GameGrid(int row, int col)
    {
        int[,] _array = new int[row,col];
        public GameGrid() : this(10,10) 
        {
        }
        public int this[int row, int col] => _array[row, col];

        public int GetCell(int row, int column)
        {
            try
            {
                return _array[row, col];
            }
            catch (IndexOutOfRangeException)
            {
                return -1;
            }
        }
        public void BombCell(int row, int col) 
        {
            if (_array[row,col] == 1) _array[row,col] = 2;
        }

    }
}
