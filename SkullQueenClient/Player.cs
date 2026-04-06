using System.IO;
using System.Net.Sockets;

namespace SkullQueenClient
{
    public class Player
    {
        private string name;
        private TcpClient connection;

        private StreamWriter writer;
        private StreamReader reader;
        public Player(string name, TcpClient connection)
        {
            this.name = name;
            this.connection = connection;
            this.writer = new StreamWriter(connection.GetStream()) { AutoFlush = true };
            this.reader = new StreamReader(connection.GetStream());
        }
        public void SendMessage(string message)
        {
            writer.WriteLine(message);
        }
        private string GetMessage()
        {
            return reader.ReadLine() ?? "";
        }

        public async void ListenForMessages(Action<string> onMessageReceived)
        {
            while (true)
            {
                string message = await reader.ReadLineAsync() ?? "";
                onMessageReceived(message);
            }
        }
    }
}