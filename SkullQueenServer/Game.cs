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
            Utility.BroadCast(players, Command.StartGame);

            // Send all players their opponents
            foreach (Player player in players)
            {
                Utility.BroadCast(players, Command.DisplayOpponent, player.name, player);
            }

            // Start the first round
            NewRound();
        }
        private void NewRound()
        {
            this.currentRound = new Round(players);
        }
    }
}