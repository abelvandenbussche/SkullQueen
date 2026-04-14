using Shared;

namespace SkullQueenServer
{
    public static class Utility
    {
        public static void BroadCast(List<Player> players, Command cmd, string? message = null, Player? exclude = null)
        {
            foreach (Player player in players)
            {
                if (exclude != player)
                {
                    player.SendMessage(cmd, message);
                }
            }
        }
        public static void DisplayPlanks(List<Player> players)
        {
            foreach (Player player in players)
            {
                player.SendMessage(Command.DisplayPlank, player.plank.ToString());
                Utility.BroadCast(players, Command.DisplayOpponentPlank, player.name + " " + player.plank.ToString(), player);
            }
        }
        public static void BroadCastMiddleCards(List<Player> players, List<Card> centerCards)
        {
            // Sending middle cards to client
            string message = "";
            foreach (Card card in centerCards)
            {
                message += card.ToString() + " ";
            }

            Utility.BroadCast(players, Command.DisplayMiddleCards, message);

        }
    }
}