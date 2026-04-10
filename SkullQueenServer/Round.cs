using System.Diagnostics;
using Shared;

namespace SkullQueenServer
{
    class Round
    {
        private Random rand = new();
        private List<Player> players;
        private Trick currentTrick;
        private Queue<Card> deck;
        private Player startPlayer;
        public Round(List<Player> players)
        {
            this.players = players;
            this.startPlayer = players[0];

            deck = CreateAndShuffleDeck();
        }
        public async Task StartRound()
        {
            DealCards();

            // Letting the players set up their boards
            Utility.BroadCast(players, Command.MakePlank);

            // Waiting on players to send their plank
            async Task GetAndSetPlank(Player player)
            {
                string message = await player.GetMessageAsync();
                string[] args = message.Split(' ');
                Plank plank = Plank.FromString(String.Join(' ', args.Skip(1).ToArray()));
                player.plank = plank;
            }

            await Task.WhenAll(players.Select(player => GetAndSetPlank(player)));

            // Starting the game loop
            GameLoop();
        }
        private void GameLoop()
        {
            while (players[0].GetCardCount() > 0)
            {
                currentTrick = new Trick(players);
                startPlayer = currentTrick.StartTrick(startPlayer);

                Utility.BroadCast(players, Command.ClearPlayedCards);
            }
        }
        public void DealCards()
        {
            while (deck.Count >= players.Count)
            {
                foreach (Player player in players)
                {
                    Card card = deck.Dequeue();
                    player.ReceiveCard(card);
                }
            }
        }
        public Queue<Card> CreateAndShuffleDeck()
        {
            List<Card> cards = new();
            foreach (Color suit in Enum.GetValues(typeof(Color)))
            {
                if (suit == Color.Black)
                {
                    continue; // skip black suit for normal cards
                }
                for (int rank = 1; rank <= 12; rank++)
                {
                    if (rank == 5 || rank == 8)
                    {
                        cards.Add(new DoubleCard(suit, rank == 8));
                    }
                    else
                    {
                        cards.Add(new Card(suit, rank));
                    }
                }
            }
            cards.Add(new BlackCard(false));
            cards.Add(new BlackCard(true));
            // shuffle the deck
            return new Queue<Card>(cards.OrderBy(x => rand.Next()));
        }
    }
}