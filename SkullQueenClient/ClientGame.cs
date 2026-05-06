using Shared;

namespace SkullQueenClient
{
    public class ClientGame
    {
        public List<Card> Hand { get; set; }
        public List<Opponent> opponents;
        public Card? playedCard;
        public List<Card> middleCards = new();
        public Color? currentLeadSuit;
        public bool freeChoice;
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
            return Hand.Any(card => card.suit == suit);
        }
        public void CheckChoice()
        {
            if (currentLeadSuit == null)
            {
                // There is no leadsuit so there is free choice
                this.freeChoice = true;
                return;
            }
            else
            {
                freeChoice = Hand.FindAll(card => card.suit == currentLeadSuit).Count == 0;
            }
            return;
        }
    }
}