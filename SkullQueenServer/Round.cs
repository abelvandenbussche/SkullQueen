using System.Diagnostics;
using System.Diagnostics.Contracts;
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
        private List<Card> centerCards;
        public Round(List<Player> players)
        {
            this.players = players;
            this.startPlayer = players[0];
            centerCards = new();

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
            Utility.DisplayPlanks(players);

            Utility.BroadCastMiddleCards(players, centerCards);

            // Starting the game loop
            GameLoop();

            // Calculating the scores of each player
            foreach (Player player in players)
            {
                foreach (Color color in player.plank.pawnsOnPlank.Keys)
                {
                    Pawn pawn = player.plank.pawnsOnPlank[color];
                    if (pawn.position == -1) { continue; }
                    player.score += player.plank.flipped ? Plank.plankScores[4 - pawn.position] : Plank.plankScores[pawn.position];
                }
                player.SendMessage(Command.ScoreUpdate, player.score.ToString());
            }
        }
        private void GameLoop()
        {
            while (players[0].GetCardCount() > 0)
            {
                Console.WriteLine(players[0].GetCardCount());
                currentTrick = new Trick(players, centerCards);
                startPlayer = currentTrick.StartTrick(startPlayer);
                centerCards = currentTrick.centerCards;

                Utility.BroadCastMiddleCards(players, centerCards);
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
            Console.WriteLine(deck.Count);

            // Adding any remaining cards to the middle
            centerCards.AddRange(deck);

            if (centerCards.Count == 2)
            {
                if (centerCards[0].suit == centerCards[1].suit)
                {
                    centerCards = new();
                }
                centerCards.RemoveAll(x => x.suit == Color.Black);
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
            // Shuffle the deck
            return new Queue<Card>(cards.OrderBy(x => rand.Next()));
        }
    }
}