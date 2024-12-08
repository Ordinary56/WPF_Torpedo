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
            var test = _host.Services.GetService<ITest>();
            await Task.WhenAll(_host.RunAsync(), server.StartAsync(), test!.Test());
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
            services.AddTransient<ITest, TestClientDisconnected>();
        }
    }

    public class Server
    {
        private readonly TcpListener _listener;
        private readonly List<(string, TcpClient)> _clients;
        private readonly ILogger<Server> _logger;

        private const string ADDRESS = "127.0.0.1";
        private const int PORT = 37065;

        public Server(ILogger<Server> logger)
        {
            _listener = new TcpListener(IPAddress.Parse(ADDRESS), PORT);
            _clients = new List<(string, TcpClient)>();
            _logger = logger;
        }

        async Task<string> GetUsernameAsync(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead == 0)
            {
                _logger.LogWarning("Client disconnected before sending any data.");
                return string.Empty;
            }
            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }

        public async Task StartAsync()
        {
            _listener.Start();
            _logger.Log(LogLevel.Information, "Server is running at {Address}:{Port}", ADDRESS, PORT);
            while (true)
            {
                TcpClient client = await _listener.AcceptTcpClientAsync();
                _logger.Log(LogLevel.Information, "Client connected from {Address}:{Port}", ((IPEndPoint)client.Client.RemoteEndPoint).Address, ((IPEndPoint)client.Client.RemoteEndPoint).Port);

                string username = await GetUsernameAsync(client);
                if (string.IsNullOrEmpty(username))
                {
                    _logger.LogWarning("Client did not send a valid username.");
                    continue; // Skip invalid connections
                }

                lock (_clients)
                {
                    _clients.Add((username, client));
                    _logger.Log(LogLevel.Information, "Client connected, total clients: {clientCount}", _clients.Count);
                }
                await WaitForConnectingPairAsync(username, client);
            }
        }

        async Task WaitForConnectingPairAsync(string username, TcpClient client)
        {
            while (client.Connected)
            {
                if ((_clients.Count & 1) == 0) // Every second client forms a pair
                {
                    TcpClient first = _clients[_clients.Count - 2].Item2;
                    TcpClient second = _clients[_clients.Count - 1].Item2;
                    Thread thread = new(() => HandleClientPair(first, second));
                    thread.Start();
                    break; // Pair found, start communication
                }
                await Task.Delay(100); // Check periodically for a pair
            }

            // Client disconnected without a pair
            lock (_clients)
            {
                _clients.Remove((username, client));
            }
            _logger.LogWarning("Client without a pair disconnected.");
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
                _clients.RemoveAll(x => x.Item2 == first || x.Item2 == second);
            }

            _logger.Log(LogLevel.Information, "Client pair disconnected.");
        }
    }
}
