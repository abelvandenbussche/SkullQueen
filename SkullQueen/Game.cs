using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;

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

        public void Host(TextBox nameField, TextBlock lobbyText, Button startButton)
        {
            string name = nameField.Text;

            //adding this player
            AddPlayer(new(name, null), lobbyText);

            // making and adding a player
            startButton.Click += (s, e) =>
            {
                cts.Cancel();
                server.Stop();
            };

            // start accepting clients
            Task clientGetter = GetPlayers(lobbyText);
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
                    string name = "something else";
                    AddPlayer(new(name, client), lobbyText);
                }
            }
            catch (ObjectDisposedException)
            {
                // stopped accepting clients
            }
        }
    }
}