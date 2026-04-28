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
            return Hand.Any(card => card.suit == suit || card.suit == Color.Black);
        }
        public void CheckChoice()
        {
            if (currentLeadSuit == null)
            {
                // There is no leadsuit so there is free choice
                this.freeChoice = true;
                return;
            }
            foreach (Card card in Hand)
            {
                if (card.suit == currentLeadSuit || card.suit == Color.Black)
                {
                    // The players has atleast 1 card with the leadsuit or black suit so they must play those
                    this.freeChoice = false;
                    return;
                }
            }
            // The players does not have any blackcards or cards with the leadsuit so there is free choice
            this.freeChoice = true;
            return;
        }
    }
}