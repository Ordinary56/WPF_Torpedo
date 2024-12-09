using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Tests;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    public class Program
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
        private readonly TcpListener _listener;
        private readonly ILogger<Server> _logger;


        private const string ADDRESS = "127.0.0.1";
        private const int PORT = 37065;
        // List to store all clients 
        readonly List<TcpClient> _clients;

        public Server(ILogger<Server> logger)
        {
            _listener = new TcpListener(IPAddress.Parse(ADDRESS), PORT);
            _clients = new List<TcpClient>();
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
                await WaitForConnectingPairAsync(client);
            }
        }
        async Task WaitForConnectingPairAsync(TcpClient client)

        {
            while (client.Connected)
            {
                await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes("1"));
                if ((_clients.Count & 1) == 0) // Every second client forms a pair
                {
                    TcpClient first = _clients[_clients.Count - 2];
                    TcpClient second = _clients[_clients.Count - 1];
                    Thread thread = new(() => HandleClientPair(first, second));
                    thread.Start();
                    break; // Pair found, start communication
                }
                await Task.Delay(100); // Check periodically for a pair
            }


            // Client disconnected without a pair
            lock (_clients)
            {
                _clients.Remove(client);
            }
            _logger.LogWarning("Client without a pair disconnected");
            await Task.CompletedTask;

        }

        void HandleClientPair(TcpClient first, TcpClient second)
        {
            using (first)
            using (second)
            {
                NetworkStream stream_1 = first.GetStream(), stream_2 = second.GetStream();
                byte[] firstBuffer = new byte[1024], secondBuffer = new byte[1024];
                int bytesRead_1, bytesRead_2;

                while (true)
                {
                    bytesRead_1 = stream_1.Read(firstBuffer, 0, firstBuffer.Length);
                    if (bytesRead_1 == 0)
                    {
                        _logger.LogWarning("Client 1 sent 0 bytes as message, closing the connection.");
                        break;
                    }
                    _logger.LogInformation("Client 1 sent: {Message}", Encoding.UTF8.GetString(firstBuffer, 0, bytesRead_1));
                    stream_2.Write(firstBuffer, 0, bytesRead_1);

                    bytesRead_2 = stream_2.Read(secondBuffer, 0, secondBuffer.Length);
                    if (bytesRead_2 == 0)
                    {
                        _logger.LogWarning("Client 2 sent 0 bytes as message, closing the connection.");
                        break;
                    }
                    _logger.LogInformation("Client 2 sent: {Message}", Encoding.UTF8.GetString(secondBuffer, 0, bytesRead_2));
                    stream_1.Write(secondBuffer, 0, bytesRead_2);
                }
            }

            // Remove clients from the list once they disconnect
            lock (_clients)
            {

                _logger.LogInformation("Removing clients....");
                // Remove both clients when they are disconnected
                _clients.RemoveAll(x => x == first || x == second);
            }

            _logger.Log(LogLevel.Information, "Client pair disconnected.");
        }
    }
}
