using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Torpedo.Models
{
    public class Player
    {
        private GameGrid _grid;
        private readonly TcpClient _client = new();
        public NetworkStream Stream => _client.GetStream();
        public GameGrid Grid {get => _grid; set => _grid = value; } 
        public string Username { get; set; } = "Teszt";

        public void ConnectToServer()
        {
            _client.Connect(IPAddress.Loopback, 5500);
            if (_client.Connected) 
            {
                SendInfoToServer();
            }
        }
        public void SendInfoToServer()
        {
            string info = $"{Username};{_grid.ToString(true)}";
            byte[] bytes = Encoding.UTF8.GetBytes(info);
            Stream.Write(bytes, 0, bytes.Length);
        }

        // this is utter garbage
        public void SendFireTo(int x, int y, NetworkStream stream)
        {
            byte[] buffer = Encoding.UTF8.GetBytes($"/{x};{y}");
            stream.Write( buffer, 0, buffer.Length );
        }
        public void RecieveFire(NetworkStream stream)
        {
            byte[] recieved = new byte[10];
            stream.Read(recieved);
            string[] coords = Encoding.UTF8.GetString(recieved).Split(';');
            int x = Convert.ToInt32(coords[0][^1]);
            int y = Convert.ToInt32(coords[1]);
            _grid.BombCell(x, y);
        }
    }
}
