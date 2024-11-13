using System.Net;
using System.Net.Sockets;
using System.Text;
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
        // Address
        const string ADDRESS = "127.0.0.1";
        // Port
        const int PORT = 37065;
        public Server()
        {
            _listener = new(IPAddress.Parse(ADDRESS),PORT);
            _clients = [];
        }

        async Task<string> GetUsernameAsync(TcpClient client) 
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            await stream.ReadAsync(buffer, 0, buffer.Length);
            await stream.FlushAsync();
            return Encoding.UTF8.GetString(buffer);
        }

        public async Task StartAsync() {
            _listener.Start();
            Console.WriteLine($"Server is running in {ADDRESS}:{PORT}");
            while(true) {
                // TODO: remove client if there's no pair and it's disconnected
                TcpClient client = await _listener.AcceptTcpClientAsync();
                string username = await GetUsernameAsync(client);
                lock (_clients)
                {
                    _clients.Add((username, client));
                    Console.WriteLine($"Client connected, total clients: {_clients.Count}");
                    Console.WriteLine($"{_clients.Count}");
                    if ((_clients.Count & 1) == 0) 
                    {
                        TcpClient first = _clients[_clients.Count - 2].Item2;
                        TcpClient second = _clients[_clients.Count - 1].Item2;
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
                    bytes_read_1 = await stream_1.ReadAsync(first_buffer);
                    if (bytes_read_1 == 0) break;
                    await stream_2.WriteAsync(first_buffer.AsMemory(0, bytes_read_1));
                    Console.WriteLine($"First client sent: {Encoding.UTF8.GetString(first_buffer)}");
                    bytes_read_2 = await stream_2.ReadAsync(second_buffer);
                    if(bytes_read_2 == 0) break;
                    await stream_1.WriteAsync(second_buffer.AsMemory(0, bytes_read_2));
                    Console.WriteLine($"Second client sent: {Encoding.UTF8.GetString(second_buffer)}");
                }
            }
            lock (_clients) 
            {
                // Remove both clients when they are disconnected
                _clients.RemoveAll(x => x.Item2 == first || x.Item2 == second);
            }
            Console.WriteLine("Client pair disconnected");
        }
    }
}