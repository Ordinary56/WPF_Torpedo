using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using System.Text;
namespace Server
{
    public class Program()
    {
        static IHost _host = Host.CreateDefaultBuilder()
                            .ConfigureServices(ConfigureServices)
                            .ConfigureLogging(ConfigureLogging)
                            .Build();

        public static async Task Main()
        {
            Server server = _host.Services.GetRequiredService<Server>();
            await server.StartAsync();
            await _host.RunAsync();
        }

        static void ConfigureLogging(HostBuilderContext context, ILoggingBuilder builder)
        {
            builder.ClearProviders();
            builder.AddConsole();
            builder.AddDebug();
        }
        static void ConfigureServices(HostBuilderContext context, IServiceCollection services) 
        {
            services.AddSingleton<Server>();
        }

    }
    public class Server
    {
        // Main server socket
        readonly TcpListener _listener;

        // List to store all clients 
        readonly List<(string, TcpClient)> _clients;

        // Address
        const string ADDRESS = "127.0.0.1";

        // Port
        const int PORT = 37065;

        // Logging information
        ILogger<Server> _logger;
        public Server()
        {
            _listener = new(IPAddress.Parse(ADDRESS), PORT);
            _clients = [];
        }
        public Server(ILogger<Server> logger) : this() { _logger = logger; }

        async Task<string> GetUsernameAsync(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            await stream.ReadAsync(buffer, 0, buffer.Length);
            await stream.FlushAsync();
            return Encoding.UTF8.GetString(buffer);
        }

        public async Task StartAsync()
        {
            _listener.Start();
            _logger.Log(LogLevel.Information, "Server is running at {Address}:{Port}", ADDRESS, PORT);
            TcpClient? client = null;
            string username;
            while (true)
            {
                client = await _listener.AcceptTcpClientAsync();
                username = await GetUsernameAsync(client);
                lock (_clients)
                {
                    _clients.Add((username, client));
                    _logger.Log(LogLevel.Information, "Client connected, total clients: {clientCount}", _clients.Count);
                }
                break;
            }
            await WaitForConnectingPairAsync(username, client!);
        }
        async Task WaitForConnectingPairAsync(string username,TcpClient client)
        {
            while(client.IsConnected())
            {
                if ((_clients.Count & 1) == 0)
                {
                    TcpClient first = _clients[_clients.Count - 2].Item2;
                    TcpClient second = _clients[_clients.Count - 1].Item2;
                    await HandleClientPairAsync(first, second);
                }
            }
            _logger.LogWarning("Client without a pair disconnected");
            _clients.Remove((username, client));
            await Task.CompletedTask;
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
                    if (bytes_read_1 == 0) 
                    {
                        _logger.LogWarning("Client 1 sent 0 bytes as message, breaking up the connection");
                        break; 
                    }
                    await stream_2.WriteAsync(first_buffer.AsMemory(0, bytes_read_1));
                    Console.WriteLine($"First client sent: {Encoding.UTF8.GetString(first_buffer)}");
                    bytes_read_2 = await stream_2.ReadAsync(second_buffer);
                    if (bytes_read_2 == 0) 
                    {
                        _logger.LogWarning("Client 1 sent 0 bytes as message, breaking up the connection");
                        break; 
                    }
                    await stream_1.WriteAsync(second_buffer.AsMemory(0, bytes_read_2));
                    Console.WriteLine($"Second client sent: {Encoding.UTF8.GetString(second_buffer)}");
                }
            }
            lock (_clients)
            {
                _logger.LogInformation("Removing clients....");
                // Remove both clients when they are disconnected
                _clients.RemoveAll(x => x.Item2 == first || x.Item2 == second);


            }
            _logger.Log(LogLevel.Information, "Client pair disconnected");
        }
    }
}