using System.Security.Cryptography;

namespace SkullQueen
{
    public class Card
    {
        public Color suit;
        protected int rank;

        public Card(Color suit, int rank)
        {
            this.suit = suit;
            this.rank = rank;
        }
    }
}