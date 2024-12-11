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
        private TcpListener _server;
        private List<TcpClient> _clients;
        private List<GameSession> _games;
        private const int Port = 5000;

        public Program()
        {
            _clients = new List<TcpClient>();
            _games = new List<GameSession>();
            _server = new TcpListener(IPAddress.Any, Port);
        }

        public void Start()
        {
            _server.Start();
            Console.WriteLine($"Server started on port {Port}");

            Task.Run(() => AcceptClientsAsync());
        }

        private async Task AcceptClientsAsync()
        {
            while (true)
            {
                TcpClient client = await _server.AcceptTcpClientAsync();
                _clients.Add(client);
                Console.WriteLine("New client connected");

                Task.Run(() => HandleClientAsync(client));
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead;

            // Send welcome message
            SendMessage(stream, "Welcome to Battleship! Waiting for another player...");

            // Wait for the "Ready" message from the player
            bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            string readyMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            if (!readyMessage.Contains("is ready"))
            {
                SendMessage(stream, "You must send 'Ready' to start.");
                return;
            }

            Console.WriteLine("Player is ready");

            // Wait until we have two clients
            if (_clients.Count % 2 == 0)
            {
                // Now both players are ready, start the game session
                var player1 = _clients[_clients.Count - 2];
                var player2 = _clients[_clients.Count - 1];

                GameSession gameSession = new GameSession(player1, player2);
                _games.Add(gameSession);

                // Notify both players that the game is starting
                SendMessage(player1.GetStream(), "Your game has started! You are Player 1.");
                SendMessage(player2.GetStream(), "Your game has started! You are Player 2.");

                // Start the game loop for this pair of players
                await StartGameAsync(gameSession);
            }
            else
            {
                // If there's an odd number of players, don't start a game yet
                SendMessage(stream, "Waiting for another player...");
            }
        }

        private async Task StartGameAsync(GameSession gameSession)
        {
            NetworkStream player1Stream = gameSession.Player1.GetStream(), player2Stream = gameSession.Player2.GetStream();
            Stack<bool> readyStack = new();
            while (!gameSession.Player1Ready || !gameSession.Player2Ready)
            {
                if (!gameSession.Player1Ready)
                {
                    // Check Player 1 readiness
                    byte[] buffer = new byte[1024];
                    int bytesRead = await player1Stream.ReadAsync(buffer, 0, buffer.Length);
                    string readyMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    if (readyMessage.Contains("is ready"))
                    {
                        gameSession.Player1Ready = true;
                        readyStack.Push(true);
                    }
                    else
                    {
                        SendMessage(player1Stream, "Waiting for opponent to be ready...");
                    }
                }

                if (!gameSession.Player2Ready)
                {
                    // Check Player 2 readiness
                    byte[] buffer = new byte[1024];
                    int bytesRead = await player2Stream.ReadAsync(buffer, 0, buffer.Length);
                    string readyMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    if (readyMessage.Contains("is ready"))
                    {
                        gameSession.Player2Ready = true;
                        readyStack.Push(true);
                    }
                    else
                    {
                        SendMessage(player2Stream, "Waiting for opponent to be ready...");
                    }
                }
                if (readyStack.Count == 2)
                {
                        SendMessage(player2Stream, "Other is Ready");
                        SendMessage(player1Stream, "Other is Ready");
                        break;
                }

                await Task.Delay(1000); // Prevent tight loop
            }

            // Now both players are ready, start the game loop
            await PlayGameLoopAsync(gameSession);
        }

        private async Task PlayGameLoopAsync(GameSession gameSession)
        {
            // Start alternating turns between players
            while (!gameSession.IsGameOver)
            {
                // Wait for Player 1's turn
                await HandlePlayerTurnAsync(gameSession, gameSession.Player1, gameSession.Player2);

                // Check if the game is over    
                if (gameSession.IsGameOver)
                    break;

                // Wait for Player 2's turn
                await HandlePlayerTurnAsync(gameSession, gameSession.Player2, gameSession.Player1);
            }

            // End of game
            SendMessage(gameSession.Player1.GetStream(), "Game Over!");
            SendMessage(gameSession.Player2.GetStream(), "Game Over!");

            // Cleanup after the game
            _games.Remove(gameSession);
            gameSession.Player1.Close();
            gameSession.Player2.Close();
        }

        private async Task HandlePlayerTurnAsync(GameSession gameSession, TcpClient currentPlayer, TcpClient opponent)
        {
            NetworkStream currentPlayerStream = currentPlayer.GetStream();
            NetworkStream opponentStream = opponent.GetStream();
            byte[] buffer = new byte[1024];

            // Send prompt for player's move
            SendMessage(currentPlayerStream, "Your turn! Please send attack coordinates (x,y):");

            int bytesRead = await currentPlayerStream.ReadAsync(buffer, 0, buffer.Length);
            string attackMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            string[] coords = attackMessage.Split(',');

            if (coords.Length != 2 || !int.TryParse(coords[0], out int x) || !int.TryParse(coords[1], out int y))
            {
                SendMessage(currentPlayerStream, "Invalid coordinates. Please try again.");
                return;
            }

            // Handle the attack on opponent's grid (this would be more complex in a real game)
            bool isHit = CheckIfHit(x, y, opponent);

            // Send feedback to current player
            SendMessage(currentPlayerStream, isHit ? "Hit!" : "Miss!");

            // Send feedback to opponent
            SendMessage(opponentStream, $"{(isHit ? "Hit" : "Miss")} on your grid at ({x},{y})");

            // Check if the opponent's ships are all sunk and if the game is over
            if (IsGameOver(opponent))
            {
                gameSession.IsGameOver = true;
            }
        }

        private bool CheckIfHit(int x, int y, TcpClient opponent)
        {
            // Dummy logic to determine if it's a hit
            // In a real game, this would check the opponent's ship placements
            return new Random().Next(0, 2) == 0; // Randomly return hit or miss
        }

        private bool IsGameOver(TcpClient opponent)
        {
            // Check if opponent has lost all their ships
            // For simplicity, assume the game ends when the opponent has no ships left
            return new Random().Next(0, 2) == 0; // Randomly decide if the game is over
        }

        private void SendMessage(NetworkStream stream, string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            stream.Write(buffer, 0, buffer.Length);
        }

        public static void Main(string[] args)
        {
            Program server = new Program();
            server.Start();

            // Keep the server running
            Console.ReadLine();
        }
    }


    public class GameSession
    {
        public TcpClient Player1 { get; }
        public TcpClient Player2 { get; }
        public bool IsGameOver { get; set; }
        public bool Player1Ready { get; set; }
        public bool Player2Ready { get; set; }

        public GameSession(TcpClient player1, TcpClient player2)
        {
            Player1 = player1;
            Player2 = player2;
            IsGameOver = false;
            Player1Ready = false;
            Player2Ready = false;
        }
    }
}
