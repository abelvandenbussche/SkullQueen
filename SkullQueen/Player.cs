namespace SkullQueen
{
    class Player
    {
        private Plank plank;
        private List<Card> hand;
        private int score;
        string name;

        public Player(string name)
        {
            this.name = name;

            this.plank = new Plank();
            this.score = 0;
            this.hand = new List<Card>();
        }
        public void ReceiveCard(Card card)
        {
            hand.Add(card);
        }
    }
}