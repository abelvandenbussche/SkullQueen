using Shared;
using System.Diagnostics;

namespace Shared
{
    public class Plank
    {
        public Dictionary<Color, Pawn> pawnsOnPlank;
        public bool flipped { get; private set; }
        public static readonly int[] plankScores = [1, 2, 3, 5, 8];
        public Plank(int redStart, int greenStart, int yellowStart, int blueStart, bool flipped)
        {
            pawnsOnPlank = new Dictionary<Color, Pawn>()
            {
                { Color.Red, new(redStart) },
                { Color.Green, new(greenStart) },
                { Color.Yellow, new(yellowStart) },
                { Color.Blue, new(blueStart) }
            };
            this.flipped = flipped;
        }
        public void MovePiece(Color color, bool forwards, bool doubleMove)
        {
            pawnsOnPlank[color].Move(forwards, doubleMove);
        }
        private int GetScoreForPawn(Pawn pawn)
        {
            if (pawn.IsOffPlank()) return 0;
            return plankScores[pawn.position];
        }
        public int GetTotalScore()
        {
            int sum = 0;
            foreach (Pawn pawn in pawnsOnPlank.Values)
            {
                sum += GetScoreForPawn(pawn);
            }
            return sum;
        }
        public override string ToString()
        {
            return $"Red: {pawnsOnPlank[Color.Red].position} Green: {pawnsOnPlank[Color.Green].position} Yellow: {pawnsOnPlank[Color.Yellow].position} Blue: {pawnsOnPlank[Color.Blue].position} Flipped: {flipped}";
        }
        public static Plank FromString(string plankString)
        {
            string[] parts = plankString.Split(' ');
            int redPos = int.Parse(parts[1]);
            int greenPos = int.Parse(parts[3]);
            int yellowPos = int.Parse(parts[5]);
            int bluePos = int.Parse(parts[7]);
            bool flipped = parts[9] == "True";
            return new Plank(redPos, greenPos, yellowPos, bluePos, flipped);
        }
    }
}