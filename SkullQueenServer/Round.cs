using System.Diagnostics;
using Shared;

namespace SkullQueenServer
{
    class Round
    {
        private Random rand = new();
        private List<Player> players;
        private Trick currentTrick;
        private Queue<Card> deck;
        public Round(List<Player> players)
        {
            this.players = players;

            deck = CreateAndShuffleDeck();
            DealCards();
            currentTrick = new Trick(players);

            // DEBUG: listen client messages
            foreach (Player player in players)
            {
                player.SendMessage(Command.PlayCard, Color.Yellow.ToString());
                Task.Run(async () =>
                {
                    while (true)
                    {
                        Console.WriteLine(player.WaitOnMessage());
                    }
                });
            }
        }

        public void DealCards()
        {
            while (deck.Count >= players.Count)
            {
                foreach (Player player in players)
                {
                    Card card = deck.Dequeue();
                    player.ReceiveCard(card);
                }
            }
        }

        public Queue<Card> CreateAndShuffleDeck()
        {
            List<Card> cards = new();
            foreach (Color suit in Enum.GetValues(typeof(Color)))
            {
                if (suit == Color.Black)
                {
                    continue; // skip black suit for normal cards
                }
                for (int rank = 1; rank <= 13; rank++)
                {
                    if (rank == 0 || rank == 13)
                    {
                        cards.Add(new BlackCard(rank == 13));
                    }
                    else if (rank == 5 || rank == 8)
                    {
                        cards.Add(new DoubleCard(suit, rank == 8));
                    }
                    else
                    {
                        cards.Add(new Card(suit, rank));
                    }
                }
            }
            // shuffle the deck
            return new Queue<Card>(cards.OrderBy(x => rand.Next()));
        }
    }
}