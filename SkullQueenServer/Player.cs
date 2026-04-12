using System.Collections.Specialized;
using System.ComponentModel;
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
        public Plank plank;
        protected List<Card> hand;
        public int score;
        public string name;

        readonly TcpClient? connection;
        StreamWriter? writer = null;
        StreamReader? reader = null;

        public Player(string name, TcpClient? connection)
        {
            this.connection = connection;
            if (connection != null)
            {
                this.writer = new StreamWriter(connection.GetStream(), Encoding.UTF8) { AutoFlush = true };
                this.reader = new StreamReader(connection.GetStream(), Encoding.UTF8);
            }

            this.name = name;

            this.plank = new Plank(0, 0, 0, 0, false); // This will need to be set by the user later, but for now we can just initialize it with dummy values
            this.score = 0;
            this.hand = new List<Card>();
        }

        public virtual string WaitOnMessage()
        {
            if (reader != null)
            {
                return reader.ReadLine();
            }
            return "";
        }
        public virtual async Task<string> GetMessageAsync()
        {
            if (reader != null)
            {
                return await reader.ReadLineAsync();
            }
            return "";
        }
        public virtual void SendMessage(Command cmd, string? message = null)
        {
            if ( writer != null)
            {
                writer.WriteLine(cmd.ToString() + " " + message);
            }
        }
        public void ReceiveCard(Card card)
        {
            hand.Add(card);
            SendMessage(Command.DealCard, card.ToString());
        }
        public void MovePieceOnPlank(Color piece, bool moveForward, bool doubleMove = false)
        {
            plank.MovePiece(piece, moveForward, doubleMove);
        }
        public void RemoveCardFromHand(Card card)
        {
            hand.Remove(card);
        }
        public int GetCardCount()
        {
            return hand.Count;
        }
        public Card FindCard(Card toFind)
        {
            Card found = hand.Find(card => card.suit == toFind.suit && card.rank ==  toFind.rank)!;
            return found;
        }
    }
}