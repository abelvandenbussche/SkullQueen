using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SkullQueenServer
{
    class Program
    {
        private static List<Task> games = new();
        static async Task Main(string[] args)
        {
            // Starting the udp listener
            _ = Task.Run(() => UdpListener());
            while (true)
            {
                // Creating a new game instance
                Console.WriteLine("New lobby created!");
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
        private static async Task UdpListener()
        {
            UdpClient server = new(5000);
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                byte[] data = server.Receive(ref remoteEP);
                string msg = Encoding.UTF8.GetString(data);
                if (msg == "DISCOVER_SKULLQUEEN_SERVER")
                {
                    byte[] response = Encoding.UTF8.GetBytes("SKULLQUEEN_SERVER_HERE");
                    await server.SendAsync(response, response.Length, remoteEP);
                }
            }
        }
    }
}