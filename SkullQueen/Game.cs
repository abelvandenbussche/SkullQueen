using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Xml.Linq;
using System.Linq;

namespace SkullQueen
{
    class Game
    {
        private List<Player> players;
        private Round currentRound;
        private TcpListener server;

        private CancellationTokenSource cts = new CancellationTokenSource();

        public Game()
        {
            players = new List<Player>();
        }
        public void AddPlayer(Player player, TextBlock lobbyText)
        {
            if (players.Count == 8)
            {
                return;
            }
            players.Add(player);

            //updating the lobby text
            lobbyText.Text += $"\n * {player.name}";
        }

        public Player Host(TextBox nameField, TextBlock lobbyText, Button startButton)
        {
            string name = nameField.Text;

            // making this player
            Player player = new(name, null);
            AddPlayer(player, lobbyText);

            // making and adding a player
            startButton.Click += (s, e) =>
            {
                cts.Cancel();
                server.Stop();
            };

            // start accepting clients
            Task clientGetter = GetPlayers(lobbyText);

            return player;
        }

        private async Task GetPlayers(TextBlock lobbyText)
        {
            // waiting on a player to join
            server = new(IPAddress.Any, 5050);
            server.Start();
            try
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    TcpClient client = await server.AcceptTcpClientAsync();
                    // getting the players name
                    Player newPlayer = new(null, client);
                    newPlayer.name = newPlayer.ReceiveTcpData();
                    AddPlayer(newPlayer, lobbyText);

                }
            }
            catch (ObjectDisposedException)
            {
                // stopped accepting clients
            }
        }

        public void StartNextRound()
        {
            currentRound = new(players);
        }
    }
}