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
            Task connectTask = lobby.ConnectToClients(cts);
            await lobby.WaitTillReady();
            Console.WriteLine("OK?");

            // Starting the game
            cts.Cancel();
            lobby.StopServer();
            await connectTask;
            Game game = lobby.StartGame();
            await game.StartGame();
        }
    }
}