using System.Diagnostics;

namespace SkullQueenServer
{
    class Round
    {
        private Random rand = new();
        private List<Player> players;
        private Trick currentTrick;

        public event EventHandler<Card> CardPlayed;
        public Round(List<Player> players)
        {
            this.players = players;
        }
    }
}