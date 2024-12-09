using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF_Torpedo.Models;

namespace WPF_Torpedo.Helpers
{
    public static class GridParser
    {

        public static GameGrid ParseGrid(string coord)
        {
            GameGrid grid = new();
            sbyte[,] data = new sbyte[10,10];
            int rowcounter = 0;
            int colcounter = 0;
            for(int i = 0; i < coord.Length; i++)
            {
                data[rowcounter, colcounter] = Convert.ToSByte(coord[i]);
                colcounter++;
                if(i % 10 == 0)
                {
                    rowcounter++;
                    colcounter = 0;
                }
            }
            grid.Grid = data;
            return grid;
        }
    }
}
