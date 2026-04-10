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
        static async Task Main(string[] args)
        {
            // Creating a new game instance
            Lobby lobby = new();
            CancellationTokenSource cts = new();
            _ = lobby.ConnectToClients(cts);

            // TEMP: Replace this with client start
            Console.WriteLine("Press Enter to start the game...");
            Console.ReadLine();
            Console.WriteLine("Starting the game...");

            // Starting the game
            cts.Cancel();
            Game game = lobby.StartGame();
            await game.StartGame();
        }
    }
}