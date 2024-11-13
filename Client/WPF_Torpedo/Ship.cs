using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Torpedo
{
    public abstract class Ship
    {
        public int Size { get; set; }
        public List<Position> Positions { get; private set; }
        public int Hits { get; private set; }

        public Ship(int size, List<Position> positions)
        {
            Size = size;
            Positions = positions;
            Hits = 0;
        }

        public bool IsHit(int row, int col)
        {
            var shotPosition = new Position(row, col);
            if (Positions.Any(pos => pos.Row == shotPosition.Row && pos.Column == shotPosition.Column))
            {
                Hits++;
                return true;
            }
            return false;
        }

        public bool IsSunk()
        {
            return Hits >= Size;
        }
    }
}
