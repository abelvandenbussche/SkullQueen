using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Shared;

namespace SkullQueenClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LobbyView lobbyView;
        private GameView gameView;
        private ClientGame game;

        private event Action<Card> CardClicked;

        private Dictionary<Shared.Color, Brush> ColorToBrush = new()
        {
            {Shared.Color.Red, Brushes.Red},
            {Shared.Color.Green, Brushes.Green},
            {Shared.Color.Blue, Brushes.Blue},
            {Shared.Color.Yellow, Brushes.Yellow},
            {Shared.Color.Black, Brushes.Black},
        };
        public MainWindow()
        {
            InitializeComponent();
            lobbyView = new LobbyView();
            gameView = new GameView();
            gameView.HandCanvas.SizeChanged += (s, e) => DisplayHand(game.Hand);
            gameView.PlayersCanvas.SizeChanged += (s, e) => DisplayOpponents(game.opponents);

            lobbyView.StartGameClicked += StartGame;
            MainContent.Content = lobbyView;
        }
        public void StartGame(object sender, string playerName)
        {
            Player? player = ConnectToServer(playerName);
            lobbyView.AddPlayerToLobby(playerName);

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
                        Dispatcher.Invoke(() =>
                        {
                            MainContent.Content = gameView;
                        });
                        game = new();
                        break;

                    case Command.DealCard:
                        Shared.Color suit = (Shared.Color)Enum.Parse(typeof(Shared.Color), args[0]);
                        int rank = int.Parse(args[1]);
                        Card newCard = new(suit, rank);
                        game.AddCardToHand(newCard);
                        DisplayHand(game.Hand);
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
                        gameView.StatusText.Text = $"It's your turn to play a card!" + (leadSuit != null ? " Lead suit: " + leadSuit.ToString() : " No lead suit");
                        await PlayCard(player, leadSuit);
                        gameView.StatusText.Text = "Waiting on other players to play";
                        break;

                    case Command.Displayopponent:
                        Opponent opponent = new(args[0]);
                        game.opponents.Add(opponent);
                        DisplayOpponents(game.opponents);
                        DisplayOpponents(game.opponents);
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
                            DisplayOpponents(game.opponents);
                        }
                        break;

                    case Command.ClearPlayedCards:
                        foreach (Opponent o in game.opponents)
                        {
                            o.playedCard = null;
                        }
                        DisplayOpponents(game.opponents);
                        gameView.PlayedCard.Children.Clear();
                        break;

                    case Command.JoinLobby:
                        Debug.WriteLine(args[0]);
                        lobbyView.AddPlayerToLobby(args[0]);
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
        private void DisplayHand(List<Card> hand)
        {
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => DisplayHand(hand));
            }
            gameView.HandCanvas.Children.Clear();
            double spaceBetween = gameView.HandCanvas.ActualWidth / (hand.Count + 1);
            for (int i = 0; i < hand.Count; i++)
            {
                Card card = hand[i];
                Grid newGrid = new()
                {
                    Height = 96,
                    Width = 60,
                    Tag = card,
                };
                newGrid.MouseLeftButtonUp += (s, e) => CardClicked?.Invoke((Card)((Grid)s).Tag);

                // Creating the rectangle for the card
                Rectangle cardRect = new()
                {
                    Width = 60,
                    Height = 96,
                    Fill = ColorToBrush[card.suit],
                    Stroke = Brushes.Black,
                };

                // Creating the text for the rank
                TextBlock rankText = new()
                {
                    Text = card.rank.ToString(),
                    Foreground = Brushes.White,
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                };
                newGrid.Children.Add(cardRect);
                newGrid.Children.Add(rankText);

                Canvas.SetLeft(newGrid, i * spaceBetween);
                gameView.HandCanvas.Children.Add(newGrid);
            }
        }
        private void DisplayOpponents(List<Opponent> opponents)
        {
            gameView.PlayersCanvas.Children.Clear();

            double spaceBetween = gameView.PlayersCanvas.ActualWidth / (opponents.Count);
            Debug.WriteLine(opponents.Count);

            for (int i = 0; i < opponents.Count; i++)
            {
                Opponent opponent = opponents[i];
                opponent.canvas.Children.Clear();

                TextBlock opponentBlock = new()
                {
                    Text = opponent.name,
                    Background = Brushes.LightGray,
                };
                opponent.canvas.Children.Add(opponentBlock);
                if (opponent.playedCard != null)
                {
                    Rectangle cardRect = new()
                    {
                        Width = 60,
                        Height = 96,
                        Fill = ColorToBrush[opponent.playedCard.suit],
                        Stroke = Brushes.Black,
                    };
                    TextBlock rankText = new()
                    {
                        Text = opponent.playedCard.rank.ToString(),
                        Foreground = Brushes.White,
                        FontSize = 16,
                    };
                    Grid cardGrid = new();
                    cardGrid.Children.Add(cardRect);
                    cardGrid.Children.Add(rankText);
                    Canvas.SetTop(cardGrid, 20);
                    opponent.canvas.Children.Add(cardGrid);
                }
                Canvas.SetLeft(opponent.canvas, i * spaceBetween + (spaceBetween / 2));
                gameView.PlayersCanvas.Children.Add(opponent.canvas);
            }
        }
        private void DisplayPlayedCard(Card card)
        {
            gameView.PlayedCard.Children.Clear();

            Grid newGrid = new()
            {
                Height = 96,
                Width = 60,
            };

            Rectangle cardRect = new()
            {
                Width = 60,
                Height = 96,
                Fill = ColorToBrush[card.suit],
                Stroke = Brushes.Black,
            };
            TextBlock rankText = new()
            {
                Text = card.rank.ToString(),
                Foreground = Brushes.White,
                FontSize = 16,
            };

            newGrid.Children.Add(cardRect);
            newGrid.Children.Add(rankText);
            gameView.PlayedCard.Children.Add(newGrid);

            // Displaying the hand without this card
            game.Hand.Remove(card);
            DisplayHand(game.Hand);
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
            DisplayPlayedCard(cardToPlay);
            CardClicked -= OnCardclicked;

            // Send the card to the server
            player.SendMessage(Command.PlayCard, cardToPlay.ToString());
        }
    }
}