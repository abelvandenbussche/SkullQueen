using System.Security.Cryptography;

namespace Shared
{
    public class Card
    {
        public Color suit;
        public int rank;

        public Card(Color suit, int rank)
        {
            this.suit = suit;
            this.rank = rank;
        }

        public override string ToString()
        {
            return $"{suit} {rank}";
        }
    }
}