using System.Collections.Concurrent;
using System.Diagnostics.Tracing;
using System.Net;
using System.Net.Sockets;
using Microsoft.Win32.SafeHandles;
using Shared;

namespace SkullQueenServer
{
    public class Lobby
    {
        private List<Player> players = new List<Player>();
        private TcpListener server;
        private ConcurrentDictionary<Player, bool> isReady = new();
        private TaskCompletionSource readyTcs = new();
        public Lobby()
        {
            server = new TcpListener(IPAddress.Any, 5050);
        }
        public Game StartGame()
        {
            // Adding a pirate king if there are only 2 players
            if (players.Count <= 2)
            {
                players.Add(new PirateKing());
            }
            // Creating a new game instance with the players in the lobby
            Game game = new Game(players);
            return game;
        }
        public async Task ConnectToClients(CancellationTokenSource cts)
        {
            server.Start();
            while (!cts.IsCancellationRequested && players.Count < 8)
            {
                try
                {
                    TcpClient client = await server.AcceptTcpClientAsync();
                    await HandleClient(client);
                }
                catch (ObjectDisposedException)
                {
                    // Expected when server stops
                }
                catch (SocketException)
                {
                    // Also expected when server stops
                }
            }
        }
        private async Task HandleClient(TcpClient client)
        {
            // Getting the players name
            byte[] buffer = new byte[1024];
            int bytesRead = await client.GetStream().ReadAsync(buffer, 0, buffer.Length);

            // The message is expected to be in the format "JoinLobby playerName"
            // That is why we split and take the second part as the player name
            string playerName = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead).Split(" ")[1].TrimEnd();

            Player newPlayer = new Player(playerName, client);
            players.Add(newPlayer);
            isReady[newPlayer] = false;
            Task.Run(() => ListenToPlayer(newPlayer));

            // Updating all players
            foreach (Player player in players)
            {
                if (player != newPlayer)
                {
                    player.SendMessage(Command.JoinLobby, playerName);
                    newPlayer.SendMessage(Command.JoinLobby, player.name);
                }
            }
            Console.WriteLine(playerName);
        }
        public Task WaitTillReady()
        {
            return readyTcs.Task;
        }
        private async Task ListenToPlayer(Player player)
        {
            while (true)
            {
                string message = await player.GetMessageAsync();
                message = message.Trim();
                if (message == Command.Ready.ToString())
                {
                    isReady[player] = true;
                    if (Ready())
                    {
                        readyTcs.SetResult();
                    }
                    return;
                }
            }
        }
        private bool Ready()
        {
            if (isReady.Count == 0) { return false; }
            foreach (bool i in isReady.Values)
            {
                if (!i)
                {
                    return false;
                }
            }
            return true;
        }
        public void StopServer() { server.Stop(); }
    }
}