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
    public class Game
    {
        private List<Player> players;
        private Round currentRound;
        private TcpListener server;

        private CancellationTokenSource cts = new CancellationTokenSource();
        public EventHandler<String> updateLobbyText;

        public Game()
        {
            players = new List<Player>();
        }
        public void AddPlayer(Player player)
        {
            if (players.Count == 8)
            {
                return;
            }
            players.Add(player);

            //updating the lobby text
            updateLobbyText?.Invoke(this, $" - {player.name}\n");
        }

        public Player Host(TextBox nameField, Button startButton)
        {
            string name = nameField.Text;

            // making this player
            Player player = new(name, null);
            AddPlayer(player);

            // making and adding a player
            startButton.Click += (s, e) =>
            {
                cts.Cancel();
                server.Stop();

                // starting the game
                currentRound = new(players);
                currentRound.DealCards();
                currentRound.StartRound();
            };

            // start accepting clients
            Task clientGetter = GetPlayers();

            return player;
        }

        private async Task GetPlayers()
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
                    AddPlayer(newPlayer);

                }
            }
            catch (ObjectDisposedException)
            {
                // stopped accepting clients
            }
            catch(SocketException)
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