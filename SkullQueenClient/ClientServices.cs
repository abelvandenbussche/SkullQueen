using Shared;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace SkullQueenClient
{
    public class ClientServices
    {
        private readonly ClientGame game;

        // Game events
        public event Action<Card>? CardClicked;
        public event Action<List<Card>>? HandUpdated;
        public event Action<List<Opponent>>? OpponentsUpdated;
        public event Action<Card>? PlayedCardUpdated;
        public event Action<string>? StatusUpdated;
        public event Action? GameStarted;
        public event Action? PlayedCardCleared;
        public event Action? MakePlank;
        public event Action<Plank>? PlankUpdated;
        public event Action<Plank>? PlankMade;
        public event Action<int>? ScoreUpdated;
        public event Action<List<Card>>? CenterCardsUpdated;
        public event Action<int, Dictionary<Opponent, int>>? EndGameScoring;

        // Lobby events
        public event Action<string>? PlayerAddedToLobby;
        public event Action<string>? PlayerLeftLobby;
        private event Action? ReadyUpped;
        private event Action? BotAdded;
        private event Action? BotRemoved;


        public ClientServices(ClientGame game)
        {
            this.game = game;
        }
        public void StartGame(string playerName)
        {
            Player? player = ConnectToServer(playerName);
            if (player == null)
            {
                // Connection failure, close the application
                throw (new("Shit crashed"));
                //return;
            }
            PlayerAddedToLobby?.Invoke(playerName);


            ReadyUpped += () => { player.SendMessage(Command.Ready); };
            BotAdded += () => { player.SendMessage(Command.AddBot); };
            BotRemoved += () => { player.SendMessage(Command.RemoveBot); };

            // Start listening for messages from the server
            Task listener = player.ListenForMessages(async message =>
            {
                message = message.Trim();
                // Splitting the message
                // Trying to parse the command
                Command? cmd = null;
                try
                {
                    string commandStr = message.Split(' ')[0];
                    cmd = (Command)Enum.Parse(typeof(Command), commandStr);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error parsing message: " + ex.Message);
                    return;
                }
                string[] args = message.Split(' ').Skip(1).ToArray();

                if (cmd == null)
                {
                    return;
                }

                // Try to get the Opponent
                Opponent? opp;
                try
                {
                    opp = game.opponents.FirstOrDefault(o => o.name == args[0]);
                }
                catch (IndexOutOfRangeException)
                {
                    opp = null;
                }

                switch (cmd)
                {
                    case Command.StartGame:
                        // Switch to the game view
                        GameStarted?.Invoke();
                        break;

                    case Command.DealCard:
                        Card newCard = Card.FromString(string.Join(' ', args));
                        game.AddCardToHand(newCard);
                        HandUpdated?.Invoke(game.Hand);
                        break;

                    case Command.PlayCard:
                        // Getting the leadsuit
                        string leadSuitString = args[0];
                        Shared.Color? leadSuit;
                        if (leadSuitString == "null")
                        {
                            leadSuit = null;
                        }
                        else
                        {
                            leadSuit = (Shared.Color)Enum.Parse(typeof(Shared.Color), leadSuitString);
                        }
                        StatusUpdated?.Invoke($"It's your turn to play a card!" + (leadSuit != null ? " Lead suit: " + leadSuit.ToString() : " No lead suit"));
                        await PlayCard(player, leadSuit);
                        StatusUpdated?.Invoke("Waiting on other players to play");
                        break;

                    case Command.DisplayOpponent:
                        Opponent opponent = new(args[0]);
                        game.opponents.Add(opponent);
                        OpponentsUpdated?.Invoke(game.opponents);
                        break;

                    case Command.DisplayPlank:
                        Plank updatedPlank = Plank.FromString(string.Join(' ', args));
                        PlankUpdated?.Invoke(updatedPlank);
                        break;

                    case Command.DisplayOpponentCard:
                        Card playedCard = Card.FromString(string.Join(' ', args.Skip(1)));
                        if (opp != null)
                        {
                            opp.playedCard = playedCard;
                            OpponentsUpdated?.Invoke(game.opponents);
                        }
                        break;

                    case Command.DisplayOpponentPlank:
                        opp!.plank = Plank.FromString(string.Join(' ', args.Skip(1).ToArray()));
                        OpponentsUpdated?.Invoke(game.opponents);
                        break;

                    case Command.ClearPlayedCards:
                        foreach (Opponent o in game.opponents)
                        {
                            o.playedCard = null;
                        }
                        OpponentsUpdated?.Invoke(game.opponents);
                        PlayedCardCleared?.Invoke();
                        break;

                    case Command.JoinLobby:
                        PlayerAddedToLobby?.Invoke(args[0]);
                        break;

                    case Command.RemoveBot:
                        PlayerLeftLobby?.Invoke(args[0]);
                        break;

                    case Command.MakePlank:
                        TaskCompletionSource<Plank> tcs = new();
                        void PlankComplete(Plank plank)
                        {
                            tcs.SetResult(plank);
                        }
                        MakePlank?.Invoke();

                        PlankMade += PlankComplete;
                        Plank plank = await tcs.Task;
                        PlankMade -= PlankComplete;

                        player.SendMessage(Command.MakePlank, plank.ToString());
                        break;

                    case Command.ScoreUpdate:
                        ScoreUpdated?.Invoke(int.Parse(args[0]));
                        break;

                    case Command.DisplayMiddleCards:
                        // Generating the cards
                        List<Card> centerCards = new();
                        if (args.Count() % 2 == 1)
                        {
                            throw new("We are fucked, shit not even");
                        }

                        for (int i = 0; i < args.Count(); i += 2)
                        {
                            string combo = string.Join(' ', new List<string>() { args[i], args[i + 1] });
                            centerCards.Add(Card.FromString(combo));
                        }
                        CenterCardsUpdated?.Invoke(centerCards);
                        break;

                    case Command.EndScoring:
                        // Getting the scoring
                        Dictionary<Opponent, int> opponentScores = new();
                        int playerScore = 0;
                        for (int i = 0; i < args.Length; i += 2)
                        {
                            int score = int.Parse(args[i + 1]);
                            if (args[i] == player.name)
                            {
                                playerScore = score;
                            }
                            else
                            {
                                // Finding the opponent
                                Opponent? localOpp = game.opponents.FirstOrDefault(x => x.name == args[i]);
                                if (localOpp == null)
                                {
                                    continue;
                                }
                                opponentScores[localOpp] = score;
                            }
                        }
                        EndGameScoring?.Invoke(playerScore, opponentScores);
                        break;
                }

            });
        }
        public void PlankMadeMethod(Plank plank)
        {
            PlankMade?.Invoke(plank);
        }
        public static Player? ConnectToServer(string playerName)
        {

            try
            {
                // Getting the server ip
                UdpClient udpClient = new();
                udpClient.EnableBroadcast = true;
                udpClient.Client.ReceiveTimeout = 3000;
                IPEndPoint serverEP = new(IPAddress.Any, 0);
                byte[] message = Encoding.UTF8.GetBytes("DISCOVER_SKULLQUEEN_SERVER");
                udpClient.Send(message, message.Length, new(IPAddress.Broadcast, 5000));

                // Listening for a response
                byte[] data = udpClient.Receive(ref serverEP);
                string response = Encoding.UTF8.GetString(data);
                if (response != "SKULLQUEEN_SERVER_HERE")
                {
                    MessageBox.Show("Error connecting to server: Could not find server in network");
                    return null;
                }
                Debug.WriteLine("Found server at: " + serverEP.Address.ToString());

                // Connecting to the server
                TcpClient client = new TcpClient(serverEP.Address.ToString(), 5050);
                Player player = new(playerName, client);
                player.SendMessage(Command.JoinLobby, playerName);
                return player;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error connecting to server: " + ex.Message);
                // Trying to connect to the server on local host (for testing purposes without network access)
                TcpClient client = new TcpClient("localhost", 5050);
                Player player = new(playerName, client);
                player.SendMessage(Command.JoinLobby, playerName);
                Debug.WriteLine(client != null);
                return player;
            }
        }
        private async Task PlayCard(Player player, Shared.Color? suit)
        {
            TaskCompletionSource<Card> tcs = new();
            void OnCardClickedButBetter(Card card)
            {
                // Check if the card is a valid play
                if (card.suit == suit || suit == null || card.suit == Shared.Color.Black || !game.HasSuit(suit))
                {
                    tcs.SetResult(card);
                }
            }

            CardClicked += OnCardClickedButBetter;
            Card cardToPlay = await tcs.Task;
            // Displaying the card on the player's area
            PlayedCardUpdated?.Invoke(cardToPlay);
            CardClicked -= OnCardClickedButBetter;

            // Send the card to the server
            player.SendMessage(Command.PlayCard, cardToPlay.ToString());
        }
        public void OnCardClicked(Card card)
        {
            CardClicked?.Invoke(card);
        }
        public void ReadyUp()
        {
            ReadyUpped?.Invoke();
        }
        public void AddBot()
        {
            BotAdded?.Invoke();
        }
        public void RemoveBot()
        {
            BotRemoved?.Invoke();
        }
    }
}