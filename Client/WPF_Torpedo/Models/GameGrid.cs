namespace WPF_Torpedo.Models
{
    /// <summary>
    /// This class represents the game's Grid
    /// <remarks>The game's grid is 10x10 by default</remarks>
    /// </summary>
    public class GameGrid(int row, int col)
    {
        sbyte[,] _array = new sbyte[row, col];
        public GameGrid() : this(10, 10)
        {
        }
        public int this[int row, int col] => _array[row, col];
        /// <summary>
        /// Get's a cell from the grid
        /// </summary>
        /// <param name="row"> the row's index</param>
        /// <param name="column">the column's index</param>
        /// <returns>cell's value, -2 otherwise</returns>
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
        /// <summary>
        /// Places the ship where it's only water
        /// <para>Note: this method fails if it overlaps with a another ship or there has been a bombing</para>
        /// <para>It's impossible to place a ship to a bombed tile because of the setup phase</para>
        /// </summary>
        /// <param name="ship">The ship that has to be placed</param>
        public void PlaceShip(Ship ship)
        {
            foreach(Position position in ship.TilePositions().Where(x => this[x.X,x.Y] == 0))
            {
                _array[position.X, position.Y] = ship.ID;
            }
        }

    }
}
