using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
namespace Server {
    public class Program {
        public static async Task Main(string[] args) {
            Server server = new();
            await server.StartAsync();
        }
    }
    public class Server {
        // Main server socket
        readonly TcpListener _listener;
        // List to store 
        readonly List<(string,TcpClient)> _clients;
        const string ADDRESS = "127.0.0.1";
        const int PORT = 37065;
        public Server()
        {
            _listener = new(IPAddress.Parse(ADDRESS),PORT);
            _clients = [];
        }

        public async Task StartAsync() {
            _listener.Start();
            Console.WriteLine($"Server is running in {ADDRESS}:{PORT}");
            while(true) {
                TcpClient client = await _listener.AcceptTcpClientAsync();
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                await stream.ReadAsync(buffer, 0, buffer.Length);
                string username = Encoding.UTF8.GetString(buffer);
                lock (_clients)
                {
                    _clients.Add((username, client));
                    Console.WriteLine($"Client connected, total clients: {_clients.Count}");
                    Console.WriteLine($"{_clients.Count}");
                    if ((_clients.Count & 1) == 0) 
                    {
                        TcpClient first = _clients[^2].Item2;
                        TcpClient second = _clients[^1].Item2;
                        _ = HandleClientPairAsync(first, second);  
                    }
                }
            }
        }

        async Task HandleClientPairAsync(TcpClient first, TcpClient second) 
        {
            using (first)
            using (second)
            {
                NetworkStream stream_1 = first.GetStream(), stream_2 = second.GetStream();
                byte[] first_buffer = new byte[1024], second_buffer = new byte[1024];
                int bytes_read_1, bytes_read_2;
                while (true)
                {
                    bytes_read_1 = await stream_1.ReadAsync(first_buffer, 0, first_buffer.Length);
                    if (bytes_read_1 == 0) break;
                    await stream_2.WriteAsync(first_buffer, 0, bytes_read_1);
                    Console.WriteLine($"First client sent: {Encoding.UTF8.GetString(first_buffer)}");
                    bytes_read_2 = await stream_2.ReadAsync(second_buffer, 0, second_buffer.Length);
                    if(bytes_read_2 == 0) break;
                    await stream_1.WriteAsync(second_buffer, 0, bytes_read_2);
                    Console.WriteLine($"Second client sent: {Encoding.UTF8.GetString(second_buffer)}");
                }
            }
            lock (_clients) 
            {
                // Remove both clients when they are disconnected
            }
            Console.WriteLine("Client pair disconnected");
        }
    }
}