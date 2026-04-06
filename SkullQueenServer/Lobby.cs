using System.Net;
using System.Net.Sockets;

namespace SkullQueenServer
{
    public class Lobby
    {
        private List<Player> players = new List<Player>();
        private CancellationTokenSource cts;
        private TcpListener server;
        public Lobby()
        {
            server = new TcpListener(IPAddress.Any, 5050);
        }
        public Game StartGame()
        {
            // stop accepting new clients
            cts.Cancel();
            server.Stop();

            // creating a new game instance with the players in the lobby
            Game game = new Game(players);
            return game;
        }

        public void GetPlayers()
        {
            cts = new();
            ConnectToClients().Wait();
        }
        private async Task ConnectToClients()
        {
            server.Start();
            while (!cts.IsCancellationRequested && players.Count < 8)
            {
                TcpClient client = await server.AcceptTcpClientAsync();
                // getting the players name
                byte[] buffer = new byte[1024];
                int bytesRead = await client.GetStream().ReadAsync(buffer, 0, buffer.Length);
                string playerName = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Player newPlayer = new Player(playerName, client);
                players.Add(newPlayer);

                Console.WriteLine(playerName);
            }
        }
    }
}