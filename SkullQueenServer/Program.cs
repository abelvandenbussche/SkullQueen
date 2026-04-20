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
            Console.WriteLine("1");
            UdpClient server = new(5000);
            Console.WriteLine("2");
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            Console.WriteLine("3");

            while (true)
            {
                byte[] data = server.Receive(ref remoteEP);
                string msg = Encoding.UTF8.GetString(data);
                Console.WriteLine(msg);
                if (msg == "DISCOVER_SKULLQUEEN_SERVER")
                {
                    byte[] response = Encoding.UTF8.GetBytes("SKULLQUEEN_SERVER_HERE");
                    await server.SendAsync(response, response.Length, remoteEP);
                }
            }
        }
    }
}