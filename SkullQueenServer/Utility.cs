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
    }
}