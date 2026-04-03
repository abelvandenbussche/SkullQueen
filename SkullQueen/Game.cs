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
        public event EventHandler<String> UpdateLobbyText;
        public event EventHandler<Card> CardPlayed;
        public event EventHandler<List<Player>> DisplayPlayers;

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
            UpdateLobbyText?.Invoke(this, $" - {player.name}\n");
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

                // displaying the players
                DisplayPlayers?.Invoke(this, players.Where(p => p != player).ToList());

                // starting the game
                currentRound = new(players);
                currentRound.CardPlayed += (s, e) => CardPlayed?.Invoke(s, e);
                currentRound.DealCards();
                currentRound.NewTrick();
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
                    
                    // sending all current players to the client
                    foreach (Player player in players)
                    {
                        Debug.WriteLine(player.name);
                        if (player != newPlayer)
                        {
                            newPlayer.SendTcpData(" - " + player.name);
                        }
                    }
                    AddPlayer(newPlayer);
                }
            }
            catch (ObjectDisposedException)
            {
                // stopped accepting clients
            }
            catch (SocketException)
            {
                // stopped accepting clients
            }
        }

        public void StartNextRound()
        {
            currentRound = new(players);
        }
        public void Broadcast(string message)
        {
            foreach(Player player in players)
            {
                player.SendTcpData(message);
            }
        }
    }
}