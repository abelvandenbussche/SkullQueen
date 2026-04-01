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
            double spaceBetween = width / cards.Count;

            //TODO: reuse canvas instead of making new
            Canvas canvas = new Canvas();
            
            for (int i = 0; i < cards.Count; i++)
            {
                Card card = cards[i];
                Rectangle newRect = new();
                newRect.Width = 50;
                newRect.Height = 100;
                newRect.Fill = Brushes.Red;
                newRect.Stroke = Brushes.Green;
                newRect.StrokeThickness = 1;

                // positioning
                Canvas.SetLeft(newRect, i * spaceBetween);

                canvas.Children.Add(newRect);
            }
            HandGroup.Content = canvas;
        }
    }
}
