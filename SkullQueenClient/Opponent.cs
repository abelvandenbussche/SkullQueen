using System.Windows.Controls;
using Shared;

namespace SkullQueenClient
{
    public class Opponent
    {
        public string name;
        public Canvas canvas;
        public Card? playedCard;
        public Plank? plank;
        public Opponent(string name)
        {
            this.name = name;
            this.canvas = new Canvas();
        }
    }
}