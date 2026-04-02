using System.Diagnostics;

namespace SkullQueen
{
    class Trick
    {
        private List<Player> players;
        private Dictionary<Player, Card> playedCards = new();
        private Player startingPlayer;
        private Random rand;
        private Color leadSuit;

        public EventHandler<Card> cardPlayed;


        public Trick(List<Player> players, Random rand)
        {
            this.players = players;
            this.rand = rand;
        }
        public async void Play()
        {
            List<Player> playerOrder = new();
            int startingPlayerIndex = rand.Next(0, players.Count);
            for (int i = 0; i < players.Count; i++)
            {
                int playerIndex = i + startingPlayerIndex;
                if (playerIndex >= players.Count)
                {
                    playerIndex -= players.Count;
                }
                playerOrder.Add(players[playerIndex]);
            }
            Debug.WriteLine("Hello world" + playerOrder.ToString());
            while (players[0].GetHandCount() > 0)
            {
                // playing a round
                foreach (Player player in playerOrder)
                {
                    Debug.WriteLine($"{player.name}'s turn");
                    Card playedCard = await player.PlayCard(leadSuit);
                    playedCards[player] = await player.PlayCard(leadSuit);
                    if (player ==  startingPlayer && playedCards[player].suit != Color.Black)
                    {
                        leadSuit = playedCards[player].suit;
                    }
                    
                }
            }
            // counting up the scores
        }
    }
}