using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SkullQueenServer
{
    class Program
    {
        static void Main(string[] args)
        {
            // Creating a new game instance
            Lobby lobby = new();
            lobby.GetPlayers();

            // TEMP: Replace this with client start
            Console.ReadLine();

            // Starting the game
            Game game = lobby.StartGame();
        }
    }
}