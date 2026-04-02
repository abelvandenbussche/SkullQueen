using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows.Controls;

namespace SkullQueen
{
    public class Player
    {
        private Plank plank;
        private List<Card> hand;
        private int score;
        public string name;
        TcpClient? connection;

        public EventHandler<List<Card>> HandUpdate;

        public Player(string name, TcpClient? connection)
        {
            this.connection = connection;
            this.name = name;

            this.plank = new Plank();
            this.score = 0;
            this.hand = new List<Card>();
        }
        public void ReceiveCard(Card card)
        {
            hand.Add(card);
            HandUpdate?.Invoke(this, hand);
        }
        
        public string ReceiveTcpData()
        {
            if (connection == null)
            {
                return "NO CONNECTION";
            }
            NetworkStream stream = connection.GetStream();
            StreamReader reader = new StreamReader(stream);
            string data = reader.ReadLine()!;

            return data;
        }
        public void SendTcpData(string data)
        {
            if (connection == null)
            {
                return;
            }
            NetworkStream stream = connection.GetStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(data + "\n");
            writer.Flush();
        }
        public int GetHandCount()
        {
            return hand.Count;
        }
    }
}