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

            // Send all players their opponents
            foreach (Player player in players)
            {
                BroadCast(Command.Displayopponent, player.name, player);
            }

            // Start the first round
            NewRound();
        }
        private void BroadCast(Command cmd, string? message = null, Player? exclude = null)
        {
            foreach (Player player in players)
            {
                if (exclude != player)
                {
                    player.SendMessage(cmd, message);
                }
            }
        }
        private void NewRound()
        {
            this.currentRound = new Round(players);
        }
    }
}