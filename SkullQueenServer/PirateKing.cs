using Shared;

namespace SkullQueenServer
{
    class PirateKing : Player
    {
        private TaskCompletionSource<string> messageTcs;
        public PirateKing() : base("PirateKing", null)
        {
                messageTcs = new TaskCompletionSource<string>();
        }

        public override void SendMessage(Command cmd, string? message = null)
        {
            // Checking wich message it is
            switch (cmd)
            {
                case Command.MakePlank:
                    messageTcs.SetResult(Command.MakePlank.ToString() + " " + new Plank(-1, -1, -1, -1, false).ToString());
                    break;
                case Command.PlayCard:
                    messageTcs.SetResult(Command.PlayCard.ToString() + " " + hand[0].ToString());
                    break;
            }
        }
        public override string WaitOnMessage()
        {
            messageTcs.Task.Wait();
            string result = messageTcs.Task.Result;
            messageTcs = new TaskCompletionSource<string>();
            return result;
        }
        public override Task<string> GetMessageAsync()
        {
            Task<string> task = messageTcs.Task;
            messageTcs = new TaskCompletionSource<string>();
            return task;
        }
    }
}