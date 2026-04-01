namespace SkullQueen
{
    class Round
    {
        private Random rand = new();
        private List<Player> players;

        public Round(List<Player> players)
        {
            this.players = players;
        }

        public void DealCards()
        {
            // making the deck
            Queue<Card> deck = new Queue<Card>();

            // the 2 black cards
            deck.Enqueue(new BlackCard(false));
            deck.Enqueue(new BlackCard(true));

            // all the rest
            for (int i = 0; i < 4; i++)
            {
                for (int cardNumber = 1; cardNumber < 12; cardNumber++)
                {
                    if (cardNumber == 5 || cardNumber == 8)
                    {
                        deck.Enqueue(new DoubleCard((Color)i, cardNumber == 8));
                    }
                    else
                    {
                        deck.Enqueue(new Card((Color)i, cardNumber));
                    }
                }
            }

            // shuffling the deck
            // ".Orderby" takes a lambda that returns a number for each value of a collection, then orders it based on those numbers
            deck = new(deck.OrderBy(x => rand.Next()).ToList());

            // dealing the deck
            while (deck.Count >= players.Count)
            {
                foreach (Player player in players)
                {
                    player.ReceiveCard(deck.Dequeue());
                }
            }
            // adding remaining cards to center
        }
    }
}