using System.Diagnostics;
using Shared;

namespace SkullQueenServer
{
    class Trick
    {
        private List<Player> players;
        private Dictionary<Card, Player> playedCards = new();
        private Player startingPlayer;
        private List<Player> playerOrder = new();
        private readonly Random rand;
        private Color? leadSuit;

        public Trick(List<Player> players)
        {
            this.rand = new Random();
            this.players = players;
        }
        public Player StartTrick(Player startingPlayer)
        {
            this.startingPlayer = startingPlayer;
            this.playerOrder = DeterminePlayerOrder();
            this.leadSuit = null;

            // Asking players to play their cards in order
            foreach (Player player in playerOrder)
            {
                player.SendMessage(Command.PlayCard, leadSuit != null ? leadSuit.ToString() : "null");
                string response = player.WaitOnMessage();
                Console.WriteLine($"Received response from {player.name}: {response}");
                // Parsing the response to get the card played
                Card playedCard = Card.FromString(response);

                // Check if this sets the lead suit
                if (leadSuit == null && playedCard.suit != Color.Black)
                {
                    leadSuit = playedCard.suit;
                }
                playedCards[playedCard] = player;

                // Display the played card to the other players
                foreach (Player otherPlayer in players)
                {
                    if (otherPlayer != player)
                    {
                        otherPlayer.SendMessage(Command.DisplayOpponentCard, $"{player.name} {playedCard}");
                    }
                }
                // Remove the played card from the player's hand
                player.RemoveCardFromHand(playedCard);
            }
            // Determing the scoring of the trick based on the played cards

            // Determine the next start player
            return DetermineStartPlayer();
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
        public void ScoreTrick()
        {
            Dictionary<Shared.Color, List<Card>> sorted = new();
            // Sorting the cards by suit
            foreach (Card playedCard in playedCards.Keys)
            {
                if (!sorted.ContainsKey(playedCard.suit))
                {
                    sorted[playedCard.suit] = new();
                }
                sorted[playedCard.suit].Add(playedCard);
            }
            foreach(List<Card> cards in sorted.Values)
            {
                if (cards.Count <= 1) continue;
                // Sort in descending order of rank
                cards.Sort((a, b) => b.rank.CompareTo(a.rank));
                Card firstCard = cards[0];
                Card lastCard = cards[cards.Count - 1];
                Player firstPlayer = playedCards[firstCard];
                Player lastPlayer = playedCards[lastCard];

                firstPlayer.MovePieceOnPlank(firstCard.suit, true);
                lastPlayer.MovePieceOnPlank(firstCard.suit, false);

                firstPlayer.SendMessage(Command.DisplayPlank, firstPlayer.plank.ToString());
                lastPlayer.SendMessage(Command.DisplayPlank, lastPlayer.plank.ToString());
            }
        }
        public Player DetermineStartPlayer()
        {
            int highestRank = -1;
            Card best = new(Color.Black, 0);
            foreach (Card card in playedCards.Keys)
            {
                if (card.rank > highestRank)
                {
                    highestRank = card.rank;
                    best = card;
                }
            }
            return playedCards[best];
        }
    }
}