using Shared;

namespace SkullQueenServer
{
    class RoboPlayer : Player
    {
        public RoboPlayer(Random rand) : base(RoboPlayer.GenerateName(rand), null) { }

        public static string GenerateName(Random rand)
        {
            return "robot_captain_v" + rand.Next(10000);
        }

        private Plank SetupPlank()
        {
            Dictionary<Color, int> OffsetPerColor = new()
            {
                {Color.Red, 0},
                {Color.Green, 0},
                {Color.Blue, 0},
                {Color.Yellow, 0},
            };
            foreach (Card card in hand)
            {
                OffsetPerColor[card.suit] += card.rank > 6.5 ? 1 : -1;
            }
            Plank plank = new(
                Math.Max(Math.Min(OffsetPerColor[Color.Red], 2), -2),
                Math.Max(Math.Min(OffsetPerColor[Color.Green], 2), -2),
                Math.Max(Math.Min(OffsetPerColor[Color.Yellow], 2), -2),
                Math.Max(Math.Min(OffsetPerColor[Color.Blue], 2), -2),
                OffsetPerColor.Values.Sum() > 0
            );
            return plank;
        }
    }
}