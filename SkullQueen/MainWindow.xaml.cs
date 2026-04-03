using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SkullQueen
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public event EventHandler<Card> CardClicked;
        public MainWindow(Player thisPlayer, Game thisGame)
        {
            InitializeComponent();
            NameScope.SetNameScope(PlayersCanvas, new NameScope());

            // event listeners

            thisPlayer.HandUpdate += HandleHandUpdate;
            thisGame.CardPlayed += HandleCardPlayed;
            thisGame.DisplayPlayers += DisplayPlayers;
        }

        public void HandleHandUpdate(object sender, List<Card> cards)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => { HandleHandUpdate(sender, cards); });
                return;
            }
            // updating the ui
            double width = HandCanvas.ActualWidth;
            double spaceBetween = (width - 50) / cards.Count;
            Debug.WriteLine(width);

            Dictionary<Color, Brush> colorToColor = new Dictionary<Color, Brush>()
            {
                { Color.Green, Brushes.Green },
                { Color.Red, Brushes.Red },
                { Color.Blue, Brushes.Blue },
                { Color.Yellow, Brushes.Yellow },
                { Color.Black, Brushes.Gray },
            };

            for (int i = 0; i < cards.Count; i++)
            {
                Card card = cards[i];

                Grid newGrid = new();

                // making a new rectangle
                Rectangle newRect = new();
                newRect.Width = 50;
                newRect.Height = 100;
                newRect.Fill = colorToColor[card.suit];
                newRect.Stroke = Brushes.Black;
                newRect.StrokeThickness = 1;

                newRect.DataContext = card;
                newRect.MouseLeftButtonUp += (object s, MouseButtonEventArgs e) => { Rectangle rect = s as Rectangle; CardClicked?.Invoke(s, rect.DataContext as Card); };

                // making some text
                TextBlock newText = new();
                newText.Text = $"{card.rank}";
                newText.Padding = new(2);

                // positioning
                newGrid.Children.Add(newRect);
                newGrid.Children.Add(newText);
                Canvas.SetLeft(newGrid, i * spaceBetween);

                HandCanvas.Children.Add(newGrid);
            }
        }
        public void HandleCardPlayed(object sender, Card playedCard)
        {
        }
        public void DisplayPlayers(object sender, List<Player> players)
        {
            double SpaceBetweenPlayers = PlayersCanvas.Width / players.Count;
            for (int i = 0; i < players.Count; i++)
            {
                Player player = players[i];

                // creating a canvas for each player
                Canvas playerCanvas = new();
                playerCanvas.Background = Brushes.Green;

                Canvas.SetTop(playerCanvas, 0);
                Canvas.SetLeft(playerCanvas, i * SpaceBetweenPlayers);

                playerCanvas.Height = 100;
                playerCanvas.Width = 100;

                PlayersCanvas.RegisterName(player.name, playerCanvas);
                PlayersCanvas.Children.Add(playerCanvas);
            }
        }
    }
}
