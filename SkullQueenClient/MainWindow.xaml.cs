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
using System.Xml.Serialization;

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
            game = new();
            lobbyView = new LobbyView();
            gameView = new GameView();
            gameView.HandCanvas.SizeChanged += (s, e) => DisplayCards(game.Hand, gameView.HandCanvas);
            gameView.PlayersCanvas.SizeChanged += (s, e) => DisplayOpponents(game.opponents);

            services = new(game);

            // Lobby events
            services.PlayerAddedToLobby += playerName => Dispatcher.Invoke(() => lobbyView.AddPlayerToLobby(playerName));
            services.PlayerLeftLobby += playerName => Dispatcher.Invoke(() => lobbyView.RemovePlayerFromLobby(playerName));
            services.GameStarted += () =>
            {
                Dispatcher.Invoke(() => MainContent.Content = gameView);
            };

            // Game events
            services.HandUpdated += hand => DisplayCards(hand, gameView.HandCanvas);
            services.OpponentsUpdated += opponents => DisplayOpponents(opponents);
            services.PlayedCardUpdated += card => DisplayPlayedCard(card);
            services.StatusUpdated += status => Dispatcher.Invoke(() => gameView.StatusText.Text = status);
            services.PlayedCardCleared += () => Dispatcher.Invoke(() => gameView.PlayedCard.Children.Clear());
            services.PlankUpdated += plank => DisplayPlank(plank);
            services.CenterCardsUpdated += cards => DisplayCards(cards, gameView.PlayingFieldMiddle);
            services.BotDifficultyChangedIn += difficulty => Dispatcher.Invoke(() => lobbyView.ChangeDifficulty(difficulty));
            services.MakePlank += async () =>
            {
                // Clearing the leftovers from the previous round
                gameView.PlayingFieldMiddle.Children.Clear();
                foreach (Opponent opp in game.opponents)
                {
                    opp.plank = null;
                }
                DisplayOpponents(game.opponents);

                Plank plank = await gameView.GetPlank(ColorToBrush);
                services.PlankMadeMethod(plank);
            };
            services.ScoreUpdated += newScore => Dispatcher.Invoke(() => gameView.ScoreText.Text = "Score: " + newScore.ToString());
            services.EndGameScoring += (int playerScore, Dictionary<Opponent, int> opponentScores) =>
            {
                MainContent.Content = new EndScreen(playerScore, opponentScores);
            };

            // Lobby events
            lobbyView.StartGameClicked += services.StartGame;
            lobbyView.ReadyUpClicked += services.ReadyUp;
            lobbyView.AddBot += services.AddBot;
            lobbyView.RemoveBot += services.RemoveBot;
            lobbyView.BotDifficultyChanged += (difficulty) => services.ChangeBotDifficulty(difficulty);
            gameView.HandUpdated += () => { DisplayCards(game.Hand, gameView.HandCanvas); };

            MainContent.Content = lobbyView;
        }
        public Grid MakeCardUI(Card card, bool hover=false)
        {
            void EditCard(Grid cardGrid)
            {
                if (!hover) return;
                cardGrid.MouseEnter += (s, e) =>
                {

                    TransformGroup transformGroup = new();
                    transformGroup.Children.Add(new ScaleTransform(1.2, 1.2, cardGrid.ActualWidth / 2, cardGrid.ActualHeight / 2));
                    transformGroup.Children.Add(new TranslateTransform(0, -cardGrid.ActualHeight / 4));
                    cardGrid.RenderTransform = transformGroup;
                };
                cardGrid.MouseLeave += (s, e) =>
                {
                    cardGrid.RenderTransform = null;
                };
            }
            if (!gameView.classicCards)
            {
                Grid cardGrid = MakeCardImageUI(card);
                EditCard(cardGrid);
                return cardGrid;
            }
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
                Foreground = card.suit == Shared.Color.Yellow ? Brushes.Black : Brushes.White,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new(2),
            };
            TextBlock flippedRankText = new()
            {
                Text = card.rank.ToString(),
                Foreground = card.suit == Shared.Color.Yellow ? Brushes.Black : Brushes.White,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new(2),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            flippedRankText.RenderTransform = new RotateTransform(180);
            flippedRankText.RenderTransformOrigin = new(0.5, 0.5);

            TextBlock centerText = new()
            {
                Text = card.rank.ToString(),
                Foreground = card.suit == Shared.Color.Yellow ? Brushes.Black : Brushes.White,
                FontWeight = FontWeights.Bold,
                FontSize = 30,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
            };

            if (card is DoubleCard doubleCard)
            {
                string arrow = doubleCard.up ? "↑" : "↓";
                rankText.Text += "\n" + arrow;
                flippedRankText.Text += "\n" + arrow;
            }
            newGrid.Children.Add(cardRect);
            newGrid.Children.Add(flippedRankText);
            newGrid.Children.Add(rankText);
            newGrid.Children.Add(centerText);
            EditCard(newGrid);
            return newGrid;
        }
        private Grid MakeCardImageUI(Card card)
        {
            Grid grid = new();
            Image cardImage = new();
            cardImage.Width = 60;
            cardImage.Height = 96;
            cardImage.Source = new BitmapImage(new Uri($"pack://application:,,,/CardImages/{card.suit.ToString()} {card.rank}.png"));
            grid.Children.Add(cardImage);
            grid.Tag = card;
            grid.MouseLeftButtonUp += (s, e) => services.OnCardClicked((Card)((Grid)s).Tag);
            return grid;
        }
        private void DisplayCards(List<Card> hand, Canvas canvas)
        {
            hand = hand.OrderBy(x => (int)x.suit).ThenBy(x => x.rank).ToList();
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => DisplayCards(hand, canvas));
            }
            canvas.Children.Clear();
            double spaceBetween = canvas.ActualWidth / (hand.Count + 1);
            double middle = canvas.ActualWidth / 2;
            // This avoids the cards being to spaced out
            spaceBetween = Math.Min(spaceBetween, 60);
            for (int i = 0; i < hand.Count; i++)
            {
                Card card = hand[i];
                Grid newGrid = MakeCardUI(card, true);
                // This places the cards in the middle of the canvas
                Canvas.SetLeft(newGrid, (i - hand.Count / 2) * spaceBetween + middle);
                canvas.Children.Add(newGrid);
            }
        }
        private void DisplayOpponents(List<Opponent> opponents)
        {
            gameView.PlayersCanvas.Children.Clear();

            double spaceBetween = gameView.PlayersCanvas.ActualWidth / (opponents.Count + 1);

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

                // Calculating the amount of space this would take up and deciding if it should be displayed simpler
                double space = (60 + 70 + 30) * opponents.Count; // 60 for the card, 70 for the plank, 30 for spacing
                bool simple = space > gameView.PlayersCanvas.ActualWidth;

                if (opponent.playedCard != null)
                {
                    Grid cardGrid = MakeCardUI(opponent.playedCard);
                    if (!simple && opponent.plank != null)
                    {
                        Canvas.SetLeft(cardGrid, -35); // Move the card to the left if the plank is also being displayed, otherwise it can be centered
                    }
                    Canvas.SetTop(cardGrid, 20);
                    opponent.canvas.Children.Add(cardGrid);
                }
                if (opponent.plank != null && (!simple || opponent.playedCard == null))
                {
                    Grid plankGrid = MakePlank(opponent.plank, 100, 70);
                    // Only move the plank to the right if the card is also being displayed, otherwise it can be centered
                    if (!simple && opponent.playedCard != null)
                    {
                        Canvas.SetLeft(plankGrid, 35);
                    }
                    Canvas.SetTop(plankGrid, 20);
                    opponent.canvas.Children.Add(plankGrid);
                }
                Canvas.SetLeft(opponent.canvas, i * spaceBetween + (spaceBetween / 2));
                gameView.PlayersCanvas.Children.Add(opponent.canvas);
            }
        }
        private void DisplayPlayedCard(Card card)
        {
            gameView.PlayedCard.Children.Clear();

            Grid newGrid = MakeCardUI(card);
            gameView.PlayedCard.Children.Add(newGrid);

            // Displaying the hand without this card
            game.Hand.Remove(card);
            DisplayCards(game.Hand, gameView.HandCanvas);
        }
        public void DisplayPlank(Plank plank)
        {
            Grid plankGrid = MakePlank(plank, 100, 70);
            gameView.PlankCanvas.Children.Clear();
            gameView.PlankCanvas.Children.Add(plankGrid);
        }
        private Grid MakePlank(Plank plank, double height, double width)
        {
            Grid plankGrid = new();
            plankGrid.ColumnDefinitions.Add(new());
            
            // Adding rows
            for (int i = 0; i < 5; i++)
            {
                plankGrid.RowDefinitions.Add(new());

                int pieceNumber = plank.flipped ? 4 - i : i;

                Rectangle rowRect = new()
                {
                    Fill = Brushes.Brown,
                    Stroke = Brushes.Black,
                    Height = height / 5,
                    Width = width,
                };
                TextBlock rowText = new()
                {
                    Text = Plank.plankScores[pieceNumber].ToString(),
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5, 0, 0, 0),
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
                    Margin = new Thickness(2, 0, 2, 0),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
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