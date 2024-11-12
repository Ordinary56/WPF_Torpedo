using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
namespace Server {
    public class Program {
        public static async void Main(string[] args) {
            Server server = new();
            await server.StartAsync();
        }
    }
    public class Server {
        TcpListener _listener;
        // take up every network interface
        const string ADDRESS = "0.0.0.0";
        const int PORT = 37065;
        ConcurrentBag<(string,TcpClient)> _bag;
        public Server()
        {
            _listener = new(IPAddress.Parse(ADDRESS),PORT);
            _bag = new();
        }

        public async Task StartAsync() {
            _listener.Start();
            while(true) {
                using TcpClient client = await _listener.AcceptTcpClientAsync();
                using NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                await stream.WriteAsync(buffer, 0, buffer.Length);
                string username = Encoding.UTF8.GetString(buffer);
                _bag.Add((username, client));
                Thread thread = new(new ParameterizedThreadStart(HandleClient));
                thread.Start(client);
            }
        }

        public void HandleClient(object? obj)
        {
            // TODO: handle client logic here

        }
    }
}