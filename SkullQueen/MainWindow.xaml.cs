using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        public MainWindow(Player thisPlayer)
        {
            InitializeComponent();

            thisPlayer.HandUpdate += HandUpdateEvent;
        }

        public void HandUpdateEvent(object sender, List<Card> cards)
        {
            // updating the ui
            double width = HandGroup.ActualWidth;
            double spaceBetween = (width - 50) / cards.Count;

            //TODO: reuse canvas instead of making new
            Canvas canvas = new();

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

                // making some text
                TextBlock newText = new();
                newText.Text = $"{card.rank}";
                newText.Padding = new(2);

                // positioning
                newGrid.Children.Add(newRect);
                newGrid.Children.Add(newText);
                Canvas.SetLeft(newGrid, i * spaceBetween);


                canvas.Children.Add(newGrid);
            }
            HandGroup.Content = canvas;
        }
    }
}
