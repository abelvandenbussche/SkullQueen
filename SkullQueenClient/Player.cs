using Shared;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;

namespace SkullQueenClient
{
    public class Player
    {
        public string name;
        private readonly TcpClient connection;

        private StreamWriter writer;
        private StreamReader reader;
        public Player(string name, TcpClient connection)
        {
            this.name = name;
            this.connection = connection;
            this.writer = new StreamWriter(connection.GetStream()) { AutoFlush = true };
            this.reader = new StreamReader(connection.GetStream());
        }
        public void SendMessage(Command cmd, string? message = null)
        {
            writer.WriteLine(cmd.ToString() + " " + message);
        }
        public async Task ListenForMessages(Action<string> onMessageReceived)
        {
            try
            {
                while (true)
                {
                    string message = await reader.ReadLineAsync() ?? "";
                    if (message == "")
                    {
                        continue;
                    }
                    try
                    {
                        onMessageReceived(message);
                    }
                    catch
                    {
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error: " + e.Message);
            }
        }
    }
}