using System.Diagnostics;
using Shared;

namespace SkullQueenServer
{
    class Trick
    {
        private List<Player> players;
        private Dictionary<Player, Card> playedCards = new();
        private Player startingPlayer;
        private List<Player> playerOrder = new();
        private readonly Random rand;
        private Color? leadSuit;

        public Trick(List<Player> players)
        {
            this.rand = new Random();
            this.players = players;
        }
        public void StartTrick(Player startingPlayer)
        {
            this.startingPlayer = startingPlayer;
            this.playerOrder = DeterminePlayerOrder();
            this.leadSuit = null;

            // Asking players to play their cards in order
            foreach (Player player in playerOrder)
            {
                leadSuit = Color.Green; // Placeholder for lead suit, should be determined by the first card played
                player.SendMessage(Command.PlayCard, leadSuit.ToString());
                string response = player.WaitOnMessage();
                Console.WriteLine($"Received response from {player.name}: {response}");
            }
        }
        public List<Player> DeterminePlayerOrder()
        {
            List<Player> order = new List<Player>();
            int startIndex = players.IndexOf(startingPlayer);
            for (int i = 0; i < players.Count; i++)
            {
                // Modulo to wrap around the list of players
                order.Add(players[(startIndex + i) % players.Count]);
            }
            return order;
        }
    }
}