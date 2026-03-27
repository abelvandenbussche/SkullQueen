using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Windows.Media;

namespace SkullQueen
{
    class Game
    {
        private List<Player> players;
        private Round currentRound;

        public Game()
        {
            players = new List<Player>();

            // starts the game
        }

        public void Host()
        {
            // starting a listener
            TcpListener listener = new(IPAddress.Any, 3030);
            List<TcpClient> clients = new();

            // waiting on a player to join

        }
        public void Join()
        {
            // should find all possible servers here
            // this will be implemented later

            // TEMP: Connecting to the server directly
            TcpClient client = new("127.0.1.1", 3030);
        }
    }
}