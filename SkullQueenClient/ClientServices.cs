using Shared;
using System.Diagnostics;
using System.Net.Sockets;
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

        // Lobby events
        public event Action<string>? PlayerAddedToLobby;

        public ClientServices(ClientGame game)
        {
            this.game = game;
        }
        public void StartGame(object sender, string playerName)
        {
            Player? player = ConnectToServer(playerName);
            PlayerAddedToLobby?.Invoke(playerName);

            if (player == null)
            {
                // Connection failure, close the application
                return;
            }

            // Start listening for messages from the server
            Task listener = player.ListenForMessages(async message =>
            {
                Debug.WriteLine($"Raw: [{message}] Length: {message?.Length}");

                foreach (char c in message)
                {
                    Debug.WriteLine($"Char: {(int)c}");
                }
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
                switch (cmd)
                {
                    case Command.StartGame:
                        // Switch to the game view
                        GameStarted?.Invoke();
                        break;

                    case Command.DealCard:
                        Shared.Color suit = (Shared.Color)Enum.Parse(typeof(Shared.Color), args[0]);
                        int rank = int.Parse(args[1]);
                        Card newCard = new(suit, rank);
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
                            Debug.WriteLine(message);
                            leadSuit = (Shared.Color)Enum.Parse(typeof(Shared.Color), leadSuitString);
                        }
                        StatusUpdated?.Invoke($"It's your turn to play a card!" + (leadSuit != null ? " Lead suit: " + leadSuit.ToString() : " No lead suit"));
                        await PlayCard(player, leadSuit);
                        StatusUpdated?.Invoke("Waiting on other players to play");
                        break;

                    case Command.Displayopponent:
                        Opponent opponent = new(args[0]);
                        game.opponents.Add(opponent);
                        OpponentsUpdated?.Invoke(game.opponents);
                        break;

                    case Command.DisplayOpponentCard:
                        Debug.WriteLine(message);
                        string opponentName = args[0];
                        Shared.Color cardSuit = (Shared.Color)Enum.Parse(typeof(Shared.Color), args[1]);
                        int cardRank = int.Parse(args[2]);
                        Card playedCard = new(cardSuit, cardRank);
                        Opponent? opp = game.opponents.FirstOrDefault(o => o.name == opponentName);
                        if (opp != null)
                        {
                            opp.playedCard = playedCard;
                            OpponentsUpdated?.Invoke(game.opponents);
                        }
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
                        Debug.WriteLine(args[0]);
                        PlayerAddedToLobby?.Invoke(args[0]);
                        break;
                }

            });
        }
        public Player? ConnectToServer(string playerName)
        {
            try
            {
                TcpClient client = new TcpClient("localhost", 5050);
                Player player = new(playerName, client);
                player.SendMessage(Command.JoinLobby, playerName);
                return player;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error connecting to server: " + ex.Message);
                return null;
            }
        }
        private async Task PlayCard(Player player, Shared.Color? suit)
        {
            TaskCompletionSource<Card> tcs = new();
            void OnCardclicked(Card card)
            {
                // Check if the card is a valid play
                if (card.suit == suit || suit == null || card.suit == Shared.Color.Black || !game.HasSuit(suit))
                {
                    tcs.SetResult(card);
                }
            }

            CardClicked += OnCardclicked;
            Card cardToPlay = await tcs.Task;
            // Displaying the card on the player's area
            PlayedCardUpdated?.Invoke(cardToPlay);
            CardClicked -= OnCardclicked;

            // Send the card to the server
            player.SendMessage(Command.PlayCard, cardToPlay.ToString());
        }
        public void OnCardClicked(Card card)
        {
            CardClicked?.Invoke(card);
        }
    }
}