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


        public Game(List<Player> players)
        {
            this.players = players;
            BroadCast("GAME START");
        }
        private void BroadCast(string message)
        {
            foreach (Player player in players)
            {
                player.SendMessage(message);
            }
        }
        private void NewRound()
        {
            this.currentRound = new Round(players);
        }
    }
}