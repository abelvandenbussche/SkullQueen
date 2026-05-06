using System.Collections.Concurrent;
using System.Diagnostics.Tracing;
using System.Net;
using System.Net.Sockets;
using Microsoft.Win32.SafeHandles;
using Shared;

namespace SkullQueenServer
{
    public class Lobby
    {
        public string? lobbyCode;
        private List<Player> players = new List<Player>();
        private ConcurrentDictionary<Player, bool> isReady = new();
        private TaskCompletionSource readyTcs = new();
        public Lobby()
        {
            lobbyCode = GenerateLobbyCode();
        }
        private string GenerateLobbyCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            Random random = new Random();
            // How this works:
            // Makes a enumerable of 6 times "chars" and then selects a random character for each item
            return new string(Enumerable.Repeat(chars, 6)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public bool IsFull()
        {
            return players.Count >= 6;
        }
        public Game StartGame()
        {
            // Adding a pirate king if there are only 2 players
            if (players.Count <= 2)
            {
                players.Add(new PirateKing());
            }
            // If there was only 1 player there should be a bot
            if (players.Count == 2)
            {
                players.Add(new RoboPlayer(new()));
            }
            // Creating a new game instance with the players in the lobby
            Game game = new Game(players);
            return game;
        }
        public void HandleClient(TcpClient client, string playerName)
        {
            Player newPlayer = new Player(playerName, client);
            players.Add(newPlayer);
            isReady[newPlayer] = false;
            _ = Task.Run(() => ListenToPlayer(newPlayer));
            newPlayer.SendMessage(Command.SendLobbyCode, this.lobbyCode);

            // Updating all players
            foreach (Player player in players)
            {
                if (player != newPlayer)
                {
                    player.SendMessage(Command.JoinLobby, playerName);
                    newPlayer.SendMessage(Command.JoinLobby, player.name);
                }
            }
        }
        public Task WaitTillReady()
        {
            return readyTcs.Task;
        }
        private async Task ListenToPlayer(Player player)
        {
            while (true)
            {
                string message = await player.GetMessageAsync();
                message = message.Trim();
                if (message == Command.Ready.ToString())
                {
                    isReady[player] = true;
                    if (Ready())
                    {
                        readyTcs.SetResult();
                    }
                    Utility.BroadCast(players, Command.Ready, player.name);
                    return;
                }
                else if (message == Command.AddBot.ToString())
                {
                    // Checking the player count 
                    if (players.Count < 6)
                    {
                        // Adding a bot
                        RoboPlayer bot = new(new());
                        Utility.BroadCast(players, Command.JoinLobby, bot.name);
                        players.Add(bot);
                    }
                }
                else if (message.Contains(Command.ChangeBotDifficulty.ToString()))
                {
                    string diff = message.Split(' ')[1];
                    // Changing the difficulty of a bot
                    foreach (Player p in players)
                    {
                        if (p is RoboPlayer bot)
                        {
                            switch (diff)
                            {
                                case "Easy":
                                    bot.difficulty = 0;
                                    break;
                                case "Medium":
                                    bot.difficulty = 1;
                                    break;
                                case "Hard":
                                    bot.difficulty = 2;
                                    break;
                            }
                            break;
                        }
                            Utility.BroadCast(players, Command.ChangeBotDifficulty, diff);
                    }
                }
                else if (message == Command.RemoveBot.ToString())
                {
                    // Removing a bot
                    for (int i = 0; i < players.Count; i++)
                    {
                        if (players[i] is RoboPlayer)
                        {
                            Utility.BroadCast(players, Command.RemoveBot, players[i].name);
                            players.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
        }
        private bool Ready()
        {
            if (isReady.Count == 0) { return false; }
            foreach (bool i in isReady.Values)
            {
                if (!i)
                {
                    return false;
                }
            }
            return true;
        }
    }
}