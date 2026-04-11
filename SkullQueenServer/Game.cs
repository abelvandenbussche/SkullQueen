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
        }
        public async Task StartGame()
        {
            for (int i = 0; i < players.Count; i++)
            {
                this.currentRound = new Round(players);
                await currentRound.StartRound();
            }
            // Sending the scores of the players for the end of the game
            string message = "";
            foreach (Player player in players)
            {
                message += player.name + " ";
                message += player.score + " ";
            }
            Utility.BroadCast(players, Command.EndScoring, message);
        }
    }
}