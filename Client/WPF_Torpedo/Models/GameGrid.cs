using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WPF_Torpedo.Models
{
    /// <summary>
    /// This class represents the game's row
    /// <remarks>The game's grid is 10x10 by default</remarks>
    /// </summary>
    public class GameGrid(int row, int col)
    {
        sbyte[,] _array = new sbyte[row, col];
        public GameGrid() : this(10, 10)
        {
        }
        public int this[int row, int col] => _array[row, col];

        public sbyte GetCell(int row, int column)
        {
            try
            {
                return _array[row, col];
            }
            catch (IndexOutOfRangeException)
            {
                return -2;
            }
        }
        public void BombCell(int row, int col)
        {
            // TODO: handle logic 
            if (this[row,col] > 0)
            {
                _array[row, col] = -1;
                return;
            }
            _array[row, col] = -2;
        }
        public void PlaceShip(Ship ship)
        {
            foreach(Position position in ship.TilePositions())
            {
                if (this[position.X,position.Y] == 0)
                {
                    _array[position.X, position.Y] = ship.ID;
                }
            }
        }

    }
}
