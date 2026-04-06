namespace SkullQueenServer
{
    public class DoubleCard : Card
    {
        public DoubleCard(Color suit, bool up) : base(suit, up ? 8 : 5) { }
    }
}