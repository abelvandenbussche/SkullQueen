using System.Diagnostics;

namespace SkullQueen
{
    class Trick
    {
        private List<Player> players;
        private Dictionary<Player, Card> playedCards = new();
        private Player startingPlayer;
        private List<Player> playerOrder = new();
        private readonly Random rand;
        private Color? leadSuit;

        public event EventHandler<Card> CardPlayed;

        public Trick(List<Player> players, Random rand)
        {
            this.players = players;
            this.rand = rand;

            // making the player order
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
            startingPlayer = players[startingPlayerIndex];
        }
        public async Task Play()
        {
            Debug.WriteLine("Hello world" + playerOrder.ToString());
            // playing a trick
            foreach (Player player in playerOrder)
            {
                Debug.WriteLine($"{player.name}'s turn");
                Card playedCard = await player.PlayCard(leadSuit);
                playedCards[player] = await player.PlayCard(leadSuit);

                CardPlayed?.Invoke(player, playedCard);

                // setting the leadsuit
                if (player ==  startingPlayer && playedCard.suit != Color.Black || leadSuit != null && playedCard.suit != Color.Black)
                {
                    leadSuit = playedCards[player].suit;
                }
            }
            // calculating how to move the pieces
        }
    }
}