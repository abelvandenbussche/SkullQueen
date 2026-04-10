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
        public List<Card> centerCards;

        public Trick(List<Player> players, List<Card> center)
        {
            this.rand = new Random();
            this.players = players;
            this.centerCards = center;
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
                response = string.Join(' ', response.Split(" ").Skip(1));
                Card playedCard = player.FindCard(Card.FromString(response));

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
            ScoreTrick();

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
        private void ScoreTrick()
        {
            Dictionary<Shared.Color, List<Card>> sorted = new();
            // Sorting the cards by suit
            foreach (Card playedCard in playedCards.Keys)
            {
                if (!sorted.ContainsKey(playedCard.suit) && playedCard.suit != Color.Black)
                {
                    sorted[playedCard.suit] = new();
                }
                if (playedCard.suit != Color.Black)
                {
                    sorted[playedCard.suit].Add(playedCard);
                }
                else if (leadSuit != null)
                {
                    if (!sorted.ContainsKey((Color)leadSuit))
                    {
                        sorted[(Color)leadSuit] = new();
                    }
                    sorted[(Color)leadSuit].Add(playedCard);
                }
            }
            // Adding the center cards
            for (int i = centerCards.Count - 1; i >= 0; i--)
            {
                Card centerCard = centerCards[i];
                if (sorted.ContainsKey(centerCard.suit))
                {
                    sorted[centerCard.suit].Add(centerCard);
                    centerCards.Remove(centerCard);
                }
            }
            foreach(List<Card> cards in sorted.Values)
            {
                // Adding the card to the center if it is the only card in its suit
                if (cards.Count == 1) { centerCards.Add(cards[0]); }

                // Sort in descending order of rank
                cards.Sort((a, b) => b.rank.CompareTo(a.rank));
                Card firstCard = cards[0];
                Card lastCard = cards[cards.Count - 1];

                // Checking for doubles
                bool doubleUp = false;
                bool doubleDown = false;
                foreach (Card card in cards)
                {
                    if (card is DoubleCard doubleCard)
                    {
                        if (doubleCard.up) { doubleUp = true; }
                        else {  doubleDown = true; }
                    }
                }

                if (playedCards.ContainsKey(firstCard))
                {
                    Player firstPlayer = playedCards[firstCard];
                    firstPlayer.MovePieceOnPlank(firstCard.suit != Color.Black ? firstCard.suit : (Color)leadSuit!, true, doubleUp);
                }
                if (playedCards.ContainsKey(lastCard))
                {
                    Player lastPlayer = playedCards[lastCard];
                    lastPlayer.MovePieceOnPlank(firstCard.suit != Color.Black ? firstCard.suit : (Color)leadSuit!, false, doubleDown);
                }


            }
            Utility.DisplayPlanks(players);
        }
        public Player DetermineStartPlayer()
        {
            int highestRank = -1;
            Card best = new(Color.Black, 0);
            foreach (Card card in playedCards.Keys)
            {
                if (card.rank >= highestRank)
                {
                    highestRank = card.rank;
                    best = card;
                }
            }
            return playedCards[best];
        }
    }
}