using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Windows.Media;
using System.Windows;

namespace SkullQueen
{
    class Game
    {
        private List<Player> players;
        private Round currentRound;

        TcpListener server;

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
        }

        public void Host()
        {
            // starting a listener
            TcpListener listener = new(IPAddress.Any, 5050);
            List<TcpClient> clients = new();

            // waiting on a player to join
            server = new(IPAddress.Any, 5050);
            server.Start();

            TcpClient client = server.AcceptTcpClient();

            // making and adding a player
            AddPlayer(new());
            Debug.WriteLine("Player joined");
        }
    }
}