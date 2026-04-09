using Shared;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.Security.Permissions;
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
using System.Windows.Threading;

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
        private ClientServices services;

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
            game = new();

            services = new(game);

            // Lobby events
            services.PlayerAddedToLobby += playerName => Dispatcher.Invoke(() => lobbyView.AddPlayerToLobby(playerName));
            services.GameStarted += () =>
            {
                Dispatcher.Invoke(() => MainContent.Content = gameView);
            };

            // Game events
            services.HandUpdated += hand => DisplayHand(hand);
            services.OpponentsUpdated += opponents => DisplayOpponents(opponents);
            services.PlayedCardUpdated += card => DisplayPlayedCard(card);
            services.StatusUpdated += status => Dispatcher.Invoke(() => gameView.StatusText.Text = status);
            services.PlayedCardCleared += () => Dispatcher.Invoke(() => gameView.PlayedCard.Children.Clear());
            services.PlankUpdated += plank => DisplayPlank(plank);

            // Lobby events
            lobbyView.StartGameClicked += services.StartGame;

            MainContent.Content = lobbyView;
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
                newGrid.MouseLeftButtonUp += (s, e) => services.OnCardClicked((Card)((Grid)s).Tag);

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
                if (opponent.plank != null)
                {
                    Grid plankGrid = MakePlank(opponent.plank);
                    Canvas.SetTop(plankGrid, 130);
                    Canvas.SetLeft(plankGrid, 20);
                    opponent.canvas.Children.Add(plankGrid);
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
        public void DisplayPlank(Plank plank)
        {
            Grid plankGrid = MakePlank(plank);
            gameView.PlankCanvas.Children.Clear();
            gameView.PlankCanvas.Children.Add(plankGrid);
        }
        private Grid MakePlank(Plank plank)
        {
            Grid plankGrid = new();
            plankGrid.ColumnDefinitions.Add(new());
            
            // Adding rows
            for (int i = 0; i < 5; i++)
            {
                plankGrid.RowDefinitions.Add(new());

                Rectangle rowRect = new()
                {
                    Fill = Brushes.Brown,
                    Stroke = Brushes.Black,
                };
                TextBlock rowText = new()
                {
                    Text = Plank.plankScores[i].ToString(),
                    Foreground = Brushes.White,
                };
                Grid.SetRow(rowRect, i);
                Grid.SetRow(rowText, i);
                Grid.SetColumnSpan(rowRect, 5);
                plankGrid.Children.Add(rowRect);
                plankGrid.Children.Add(rowText);
            };
            for (int i = 0; i < 4; i++)
            {
                plankGrid.ColumnDefinitions.Add(new() { Width = new GridLength(1, GridUnitType.Star) });
                Shared.Color color = (Shared.Color)i;
                int pos = plank.pawnsOnPlank[color].position;
                Rectangle pieceRect = new()
                {
                    Width = 10,
                    Height = 10,
                    Margin = new Thickness(2, 0, 2, 0)
                };
                if (pos == -1)
                {
                    pieceRect.Fill = Brushes.Transparent;
                    Grid.SetRow(pieceRect, 0);
                }
                else
                {
                    pieceRect.Fill = ColorToBrush[color];
                    Grid.SetRow(pieceRect, pos);
                }
                Grid.SetColumn(pieceRect, i + 1);
                plankGrid.Children.Add(pieceRect);
            }
            return plankGrid;
        }
    }
}