using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Torpedo.Models
{
    // TODO: implement player class
    public class Player
    {
        private readonly GameGrid _grid;
        public GameGrid Grid => _grid;

        public Player(GameGrid grid)
        {
            _grid = grid;
        }
        public void SendFireTo(int x, int y)
        {

        }
        public void RecieveFire(int x, int y)
        {
            _grid.BombCell(x, y);
        }
    }
}
