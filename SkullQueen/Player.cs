using System.IO;
using System.Net.Sockets;
using System.Text;

namespace SkullQueen
{
    class Player
    {
        private Plank plank;
        private List<Card> hand;
        private int score;
        public string name;
        TcpClient? connection;

        public Player(string name, TcpClient? connection)
        {
            this.name = name;
            this.connection = connection;

            this.plank = new Plank();
            this.score = 0;
            this.hand = new List<Card>();
        }
        public void ReceiveCard(Card card)
        {
            hand.Add(card);
        }
        
        public string ReceiveTcpData()
        {
            if (connection == null)
            {
                return "NO CONNECTION";
            }
            NetworkStream stream = connection.GetStream();
            StreamReader reader = new StreamReader(stream);
            string data = reader.ReadLine();

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
        }
    }
}