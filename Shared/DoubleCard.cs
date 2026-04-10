namespace Shared
{
    public class DoubleCard : Card
    {
        public bool up { get; private set; }
        public DoubleCard(Color suit, bool up) : base(suit, up ? 8 : 5)
        {
            this.up = up;
        }
    }
}