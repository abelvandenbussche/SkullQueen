using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SkullQueenServer
{
    class Program
    {
        private static List<Task> games = new();
        static async Task Main(string[] args)
        {
            while (true)
            {
                // Creating a new game instance
                Lobby lobby = new();
                CancellationTokenSource cts = new();
                Task connectTask = lobby.ConnectToClients(cts);
                await lobby.WaitTillReady();
                Console.WriteLine("OK?");

                // Starting the game
                cts.Cancel();
                lobby.StopServer();
                await connectTask;
                Game game = lobby.StartGame();
                games.Add(Task.Run(() => game.StartGame()));
            }
        }
    }
}