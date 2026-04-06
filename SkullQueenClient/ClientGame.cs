using SkullQueenServer;

namespace SkullQueenClient
{
    class ClientGame
    {
        public List<Card> Hand { get; set; }
        public ClientGame()
        {
            Hand = new List<Card>();
        }
        public void AddCardToHand(Card card)
        {
            Hand.Add(card);
        }
    }
}