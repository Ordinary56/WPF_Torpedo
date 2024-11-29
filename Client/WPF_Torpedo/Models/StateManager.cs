using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Torpedo.Models
{
    internal enum States
    {
        IDLE = 0,
        IN_QUEUE,
        PREPARING,
        PLAYER_1_TURN,
        PLAYER_2_TURN,
        WIN,
        LOSE
    }
    public class StateManager
    {
    }
}
