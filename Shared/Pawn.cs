using Shared;

namespace Shared
{
    public class Pawn
    {
        Color color;
        // Position on the plank, 0 is the start, 4 is the end
        public int position { get; private set; }
        public Pawn(Color color, int position)
        {
            this.color = color;
            this.position = position;
        }

        public void Move(bool forward, bool doubleMove)
        {
            if (IsOffPlank()) return;
            int moveAmount = doubleMove ? 2 : 1;
            moveAmount *= forward ? 1 : -1;
            position += moveAmount;

            if (position < 0 || position > 4)
            {
                // The pawn has fallen of the plank
                position = -1;
            }
        }
        public bool IsOffPlank()
        {
            return position == -1;
        }
    }
}