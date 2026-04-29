using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private static TcpListener server = new TcpListener(IPAddress.Any, 5050);

        private static List<Task> games = new();
        private static List<Lobby> lobbies = new();
        private static readonly object lobbyLock = new();
        private static readonly object gameLock = new();
        static async Task Main(string[] args)
        {

            // Starting the background tasks for listening to UDP broadcasts and accepting TCP connections
            _ = Task.Run(() => UdpListener());

            server.Start();
            while (true)
            {
                try
                {
                    TcpClient client = await server.AcceptTcpClientAsync();
                    // Getting the players name
                    byte[] buffer = new byte[1024];
                    int bytesRead = await client.GetStream().ReadAsync(buffer, 0, buffer.Length);

                    // The message is expected to be in the format "JoinLobby playerName
                    // That is why we split and take the second part as the player name
                    string[] message = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead).Split(" ");

                    Console.WriteLine("Message end");
                    string playerName = message[1].TrimEnd();
                    string lobbyCode = message[2].TrimEnd();

                    // Finding a lobby with the given lobby code
                    Lobby? lobby;
                    lock (lobbyLock)
                    {
                        foreach(Lobby l in lobbies)
                        {
                            Console.WriteLine($"Lobby code: [{l.lobbyCode}], given code: [{lobbyCode}]");
                        }
                        lobby = lobbies.FirstOrDefault(l => lobbyCode.Equals(l.lobbyCode, StringComparison.OrdinalIgnoreCase));
                        if (lobby == null)
                        {
                            Console.WriteLine("New lobby created!");
                            lobby = new Lobby();
                            lobbies.Add(lobby);
                            _ = Task.Run(() => WaitOnLobby(lobby));
                        }
                    }
                    if (lobby.IsFull())
                    {
                        Console.WriteLine("New lobby created!");
                        lobby = new();
                        lock (lobbyLock)
                        {
                            lobbies.Add(lobby);
                        }
                    }
                    lobby.HandleClient(client, playerName);

                }
                catch (ObjectDisposedException)
                {
                    // Expected when server stops
                }
                catch (SocketException)
                {
                    // Also expected when server stops
                }
            }
        }
        private static async Task WaitOnLobby(Lobby lobby)
        {
            await lobby.WaitTillReady();
            lock (lobbyLock)
            {
                lobbies.Remove(lobby);
            }

            // Starting the game
            Game game = lobby.StartGame();
            lock (gameLock)
            {
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