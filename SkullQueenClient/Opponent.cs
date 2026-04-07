using System.Windows.Controls;
using Shared;

namespace SkullQueenClient
{
    class Opponent
    {
        public string name;
        public Grid grid;
        public Card? playedCard;
        public Plank? plank;
        public Opponent(string name)
        {
            this.name = name;
            this.grid = new Grid();
        }
    }
}