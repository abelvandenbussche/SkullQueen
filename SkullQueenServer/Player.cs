using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace SkullQueenServer
{
    public class Player
    {
        private Plank plank;
        private List<Card> hand;
        private int score;
        public string name;

        TcpClient connection;
        StreamWriter writer;
        StreamReader reader;

        public Player(string name, TcpClient connection)
        {
            this.connection = connection;
            this.writer = new StreamWriter(connection.GetStream(), Encoding.UTF8) { AutoFlush = true };
            this.reader = new StreamReader(connection.GetStream(), Encoding.UTF8);

            this.name = name;

            this.plank = new Plank();
            this.score = 0;
            this.hand = new List<Card>();
        }

        public string WaitOnMessage()
        {
            return reader.ReadLine();
        }
        public void SendMessage(Command cmd, string? message = null)
        {
            writer.WriteLine(cmd.ToString() + " " + message);
        }
        public void ReceiveCard(Card card)
        {
            hand.Add(card);
            SendMessage(Command.DealCard, card.ToString());
        }
    }
}