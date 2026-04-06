using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SkullQueenServer
{
    public class Player
    {
        private Plank plank;
        private List<Card> hand;
        private int score;
        public string name;
        TcpClient? connection;

        public Player(string name, TcpClient connection)
        {
            this.connection = connection;
            this.name = name;

            this.plank = new Plank();
            this.score = 0;
            this.hand = new List<Card>();
        }
    }
}