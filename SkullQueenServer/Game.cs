using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Xml.Linq;
using System.Linq;

namespace SkullQueenServer
{
    public class Game
    {
        private List<Player> players;
        private Round currentRound;
        private TcpListener server;


        public Game()
        {
            players = new List<Player>();
        }
    }
}