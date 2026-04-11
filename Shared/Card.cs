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
            Color suit = (Color)Enum.Parse(typeof(Color), parts[0]);
            int rank = int.Parse(parts[1]);
            if (rank == 8 || rank == 5)
            {
                return new DoubleCard(suit, rank == 8);
            }
            else if (rank == 0 || rank == 13)
            {
                return new BlackCard(rank == 13);
            }
            return new Card(suit, rank);
        }
    }
}