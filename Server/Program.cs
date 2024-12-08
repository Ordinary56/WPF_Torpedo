using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Tests;
using System.Net;
using System.Net.Sockets;
using System.Text;
namespace Server
{
    public class Program()
    {

        public static async Task Main()
        {
            using IHost _host = Host.CreateDefaultBuilder()
                                    .ConfigureServices(ConfigureServices)
                                    .ConfigureLogging(ConfigureLogging)
                                    .Build();
            Server server = _host.Services.GetRequiredService<Server>();
            Console.CancelKeyPress += async (o, e) =>
            {
                e.Cancel = true;
                await _host.StopAsync();
            };
            _host.Start();
            await Task.WhenAny(_host.WaitForShutdownAsync(),server.StartAsync());
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
        readonly List<TcpClient> _clients;

        // Address
        const string ADDRESS = "127.0.0.1";

        // Port
        const int PORT = 37065;

        // Logging information
        ILogger<Server> _logger;
        public Server(ILogger<Server> logger)
        {
            _listener = new(IPAddress.Parse(ADDRESS), PORT);
            _clients = [];
            _logger = logger;
        }

      public async Task StartAsync()
        {
            _listener.Start();
            _logger.Log(LogLevel.Information, "Server is running at {Address}:{Port}", ADDRESS, PORT);
            TcpClient? client = null;
            while (true)
            {
                client = await _listener.AcceptTcpClientAsync();
                lock (_clients)
                {
                    _clients.Add(client);
                    _logger.Log(LogLevel.Information, "Client connected, total clients: {clientCount}", _clients.Count);
                }
                break;
            }
            await WaitForConnectingPairAsync(client!);
        }
        async Task WaitForConnectingPairAsync(TcpClient client)
        {
            while(client.IsConnected())
            {
                if ((_clients.Count & 1) == 0)
                {
                    TcpClient first = _clients[_clients.Count - 2];
                    TcpClient second = _clients[_clients.Count - 1];
                    Thread thread = new(() => HandleClientPair(first, second));
                    thread.Start();
                }
            }
            _logger.LogWarning("Client without a pair disconnected");
            _clients.Remove(client);
            await Task.CompletedTask;
        }

        void HandleClientPair(TcpClient first, TcpClient second)
        {
            using (first)
            using (second)
            {
                NetworkStream stream_1 = first.GetStream(), stream_2 = second.GetStream();
                byte[] first_buffer = new byte[1024], second_buffer = new byte[1024];
                int bytes_read_1, bytes_read_2;
                while (true)
                {
                    bytes_read_1 = stream_1.Read(first_buffer);
                    if (bytes_read_1 == 0)
                    {
                        _logger.LogWarning("Client 1 sent 0 bytes as message, breaking up the connection");
                        break;
                    }
                    _logger.LogInformation("Client 1 sent: {Message}", Encoding.UTF8.GetString(first_buffer));
                    stream_2.Write(first_buffer);
                    bytes_read_2 = stream_2.Read(second_buffer);
                    if (bytes_read_2 == 0)
                    {
                        _logger.LogWarning("Client 1 sent 0 bytes as message, breaking up the connection");
                        break;
                    }
                    stream_1.Write(second_buffer);
                    _logger.LogInformation("Client 2 sent: {Message}", Encoding.UTF8.GetString(first_buffer));
                }
            }
            lock (_clients)
            {
                _logger.LogInformation("Removing clients....");
                // Remove both clients when they are disconnected
                _clients.RemoveAll(x => x == first || x == second);
            }
            _logger.Log(LogLevel.Information, "Client pair disconnected");
        }
    }
}