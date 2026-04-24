using Shared;
using System.Diagnostics;

namespace SkullQueenServer
{
    class RoboPlayer : Player
    {
        private TaskCompletionSource<string>? pendingResponse;
        private int difficulty = 0;
        private Random rand;
        public RoboPlayer(Random rand) : base(RoboPlayer.GenerateName(rand), null)
        {
            this.rand = rand;
        }

        public static string GenerateName(Random rand)
        {
            List<string> prefixes = new List<string>() { "Cap", "Blk", "OneEye", "Red", "Iron", "Salt", "Mad", "Blood", "Storm", "Grim" };
            List<string> cores = new List<string>() { "Jack", "Morg", "Flint", "Rack", "Silv", "Vane", "Drak", "Bon", "Kidd", "Teach" };
            List<string> appendixes = new List<string>() { "Ruth", "Curse", "Seven", "Dread", "Kraken", "Ghost", "Storm", "Tide", "Wolf", "Hook" };

            string prefix = prefixes[rand.Next(prefixes.Count)];
            string core = cores[rand.Next(cores.Count)];
            string appendix = appendixes[rand.Next(appendixes.Count)];
            return prefix + core + appendix;
        }

        private Plank SetupPlank()
        {
            Dictionary<Color, int> piecePositions = new()
            {
                {Color.Red, 0},
                {Color.Green, 0},
                {Color.Blue, 0},
                {Color.Yellow, 0},
            };
            foreach (Card card in hand)
            {
                if (card.suit == Color.Black)
                {
                    continue;
                }
                piecePositions[card.suit] += card.rank > 6.5 ? 1 : -1;
            }
            bool flipped = piecePositions.Values.Sum() > 0;
            // Limiting
            foreach (Color color in piecePositions.Keys)
            {
                int value = piecePositions[color];
                value = Math.Max(Math.Min(value, 2), -2);
                value = 2 + value;
                piecePositions[color] = value;
            }
            Plank plank = new(
                piecePositions[Color.Red],
                piecePositions[Color.Green],
                piecePositions[Color.Yellow],
                piecePositions[Color.Blue],
                flipped
            );
            return plank;
        }
        private Card PlayCard(Color? leadSuit)
        {
            List<Card> possible = hand.FindAll(card => card.suit == leadSuit || card.suit == Color.Black || leadSuit == null);
            if (possible.Count == 0)
            {
                possible = hand;
            }

            switch (difficulty)
            {
                // Default and difficulty 0
                default:
                    // Just play a random card
                    return possible[rand.Next(possible.Count)];
            }
        }
        public override void SendMessage(Command cmd, string? message = null)
        {
            switch (cmd)
            {
                case Command.MakePlank:
                case Command.PlayCard:
                    if (pendingResponse != null && !pendingResponse.Task.IsCompleted)
                    {
                        throw new("Fucking shit");
                    }
                    pendingResponse = new();

                    if (cmd == Command.MakePlank)
                    {
                        pendingResponse.SetResult(Command.MakePlank.ToString() + " " + SetupPlank().ToString());
                    }
                    else
                    {
                        string leadSuitString = message!.Split(' ')[0];
                        Color? leadSuit;
                        if (leadSuitString == "null")
                        {
                            leadSuit = null;
                        }
                        else
                        {
                            leadSuit = (Color)Enum.Parse(typeof(Color), leadSuitString);
                        }

                        pendingResponse.SetResult(Command.PlayCard.ToString() + " " + PlayCard(leadSuit).ToString());
                    }
                    // Simulating delays that normal players would have
                    Thread.Sleep(500);
                    break;
            }
        }
        public override string WaitOnMessage()
        {
            string result = pendingResponse!.Task.Result;
            pendingResponse = null;
            return result;
        }
        public override async Task<string> GetMessageAsync()
        {
            TaskCompletionSource<string> response = pendingResponse!;
            string result = await response.Task;
            pendingResponse = null;
            return result;
        }
    }
}