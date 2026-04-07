using Shared;

namespace SkullQueenClient
{
    class ClientGame
    {
        public List<Card> Hand { get; set; }
        public List<Opponent> opponents;
        public ClientGame()
        {
            Hand = new List<Card>();
            opponents = new List<Opponent>();
        }
        public void AddCardToHand(Card card)
        {
            Hand.Add(card);
        }
        public bool HasSuit(Color? suit)
        {
            return Hand.Any(card => card.suit == suit || card.suit == Color.Black);
        }
    }
}