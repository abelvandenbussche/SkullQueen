namespace Shared
{
    public class BlackCard : Card
    {
        public BlackCard(bool thirtheen) : base(Color.Black, thirtheen ? 13  : 0) { }
    }
}