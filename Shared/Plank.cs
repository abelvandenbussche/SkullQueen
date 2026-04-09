using Shared;
using System.Diagnostics;

namespace Shared
{
    public class Plank
    {
        public Dictionary<Color, Pawn> pawnsOnPlank;

        public static readonly int[] plankScores = [1, 2, 3, 5, 8];
        public Plank(int redStart, int greenStart, int yellowStart, int blueStart)
        {
            pawnsOnPlank = new Dictionary<Color, Pawn>()
            {
                { Color.Red, new(Color.Red, redStart) },
                { Color.Green, new(Color.Green, greenStart) },
                { Color.Yellow, new(Color.Yellow, yellowStart) },
                { Color.Blue, new(Color.Blue, blueStart) }
            };
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
            return $"Red: {pawnsOnPlank[Color.Red].position} Green: {pawnsOnPlank[Color.Green].position} Yellow: {pawnsOnPlank[Color.Yellow].position} Blue: {pawnsOnPlank[Color.Blue].position}";
        }
        public static Plank FromString(string plankString)
        {
            string[] parts = plankString.Split(' ');
            Debug.WriteLine($"Parsing plank string: {plankString}");
            int redPos = int.Parse(parts[1]);
            int greenPos = int.Parse(parts[3]);
            int yellowPos = int.Parse(parts[5]);
            int bluePos = int.Parse(parts[7]);
            return new Plank(redPos, greenPos, yellowPos, bluePos);
        }
    }
}