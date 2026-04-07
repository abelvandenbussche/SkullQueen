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

        public static Card FromString(string cardString)
        {
            string[] parts = cardString.Split(' ');
            Color suit = (Color)Enum.Parse(typeof(Color), parts[1]);
            int rank = int.Parse(parts[2]);
            return new Card(suit, rank);
        }
    }
}