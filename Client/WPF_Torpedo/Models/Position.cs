using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Torpedo.Models
{
    public record struct Position
    {
        public sbyte X { get; set; }
        public sbyte Y { get; set; }
    }
}
