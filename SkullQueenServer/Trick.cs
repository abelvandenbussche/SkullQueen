using System.Diagnostics;

namespace SkullQueen
{
    class Trick
    {
        private List<Player> players;
        private Dictionary<Player, Card> playedCards = new();
        private Player startingPlayer;
        private List<Player> playerOrder = new();
        private readonly Random rand;
        private Color? leadSuit;

    }
}