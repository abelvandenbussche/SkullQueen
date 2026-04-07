using System.Diagnostics.Tracing;
using System.Net;
using System.Net.Sockets;
using Shared;

namespace SkullQueenServer
{
    public class Lobby
    {
        private List<Player> players = new List<Player>();
        private TcpListener server;
        public Lobby()
        {
            server = new TcpListener(IPAddress.Any, 5050);
        }
        public Game StartGame()
        {
            // Creating a new game instance with the players in the lobby
            Game game = new Game(players);
            return game;
        }
        public async Task ConnectToClients(CancellationTokenSource cts)
        {
            server.Start();
            while (!cts.IsCancellationRequested && players.Count < 8)
            {
                TcpClient client = await server.AcceptTcpClientAsync();
                // Getting the players name
                byte[] buffer = new byte[1024];
                int bytesRead = await client.GetStream().ReadAsync(buffer, 0, buffer.Length);
                // The message is expected to be in the format "JoinLobby playerName"
                // That is why we split and take the second part as the player name
                string playerName = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead).Split(" ")[1];

                Player newPlayer = new Player(playerName, client);
                players.Add(newPlayer);

                // Updating all players
                foreach (Player player in players)
                {
                    if (player != newPlayer)
                    {
                        player.SendMessage(Command.JoinLobby, playerName);
                        newPlayer.SendMessage(Command.JoinLobby, player.name);
                    }
                }
                Console.Write(playerName);
            }
        }
    }
}