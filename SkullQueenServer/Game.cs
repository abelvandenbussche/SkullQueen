using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Xml.Linq;
using System.Linq;
using Shared;

namespace SkullQueenServer
{
    public class Game
    {
        private List<Player> players;
        private Round currentRound;


        public Game(List<Player> players)
        {
            this.players = players;
            BroadCast(Command.StartGame);
            NewRound();
        }
        private void BroadCast(Command cmd, string? message = null)
        {
            foreach (Player player in players)
            {
                player.SendMessage(cmd.ToString() + message);
            }
        }
        private void NewRound()
        {
            this.currentRound = new Round(players);
        }
    }
}