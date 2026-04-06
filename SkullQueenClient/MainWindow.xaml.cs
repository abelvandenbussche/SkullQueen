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
            Task listener = player.ListenForMessages(message =>
            {
            // splitting the message
            // trying to parse the command
            object cmd = null;
            try
            {
                string commandStr = message.Split(' ')[0];
                cmd = Enum.Parse(typeof(Command), commandStr);
                cmd = cmd as Command? ?? throw new Exception("Invalid command");
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

                    case Command.JoinLobby:
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
                player.SendMessage(playerName);
                return player;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error connecting to server: " + ex.Message);
                return null;
            }
        }
        public void DisplayHand(List<Card> hand)
        {
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => DisplayHand(hand));
            }
            gameView.HandCanvas.Children.Clear();
            double spaceBetween = gameView.HandCanvas.ActualWidth / (hand.Count + 1);
            for (int i = 0; i < hand.Count; i++)
            {
                Grid newGrid = new()
                {
                    Height = 96,
                    Width = 60,
                };
                Card card = hand[i];

                // creating the rectangle for the card
                Rectangle cardRect = new()
                {
                    Width = 60,
                    Height = 96,
                    Fill = ColorToBrush[card.suit],
                    Stroke = Brushes.Black,
                };

                // creating the text for the rank
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
    }
}