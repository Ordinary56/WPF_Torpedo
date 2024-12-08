using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Torpedo.Models
{
    public enum GAME_STATE  {
        SINGLE = 0,
        MULTI = 1
    }

    public enum PLAYER_STATE
    {
        IN_TURN = 0,
        NOT_IN_TURN,
        WIN,
        //chat, is this loss?
        LOSS
    }
    public class StateManager : INotifyPropertyChanged
    {
        private GAME_STATE? _selectedgame = null;
        private PLAYER_STATE _playerstate;
        public GAME_STATE? SelectedGame
        {
            get => _selectedgame;
            set
            {
                _selectedgame = value;
                OnPropertyChanged(nameof(SelectedGame));
            }
        }
        public PLAYER_STATE PlayerState
        {
            get => _playerstate;
            set
            {
                _playerstate = value;
                OnPropertyChanged(nameof(PlayerState));
            }
        }
        //#region Interface implementation 
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        //#endregion 
    }
}
