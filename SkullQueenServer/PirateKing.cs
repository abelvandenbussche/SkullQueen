using Shared;

namespace SkullQueenServer
{
    class PirateKing : Player
    {
        private string nextMessage = "";
        public PirateKing() : base("PirateKing", null) { }

        public override void SendMessage(Command cmd, string? message = null)
        {
            // Checking wich message it is
            switch (cmd)
            {
                case Command.MakePlank:
                    nextMessage = Command.MakePlank.ToString() + " " + new Plank(-1, -1, -1, -1, false).ToString();
                    break;
                case Command.PlayCard:
                    nextMessage = Command.PlayCard.ToString() + " " + hand[0].ToString();
                    break;
            }
        }
        public override string WaitOnMessage()
        {
            return nextMessage;
        }
        public override Task<string> GetMessageAsync()
        {
            return Task.FromResult(nextMessage);
        }
    }
}