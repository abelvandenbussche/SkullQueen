using Shared;

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
            return "robot_captain_v" + rand.Next(10000);
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