using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SkullQueenClient
{
    /// <summary>
    /// Interaction logic for GameView.xaml
    /// </summary>
    public partial class GameView : UserControl
    {
        private event Action<int>? plankSectionClicked;
        public GameView()
        {
            InitializeComponent();
        }
        public Plank MakePlank(Dictionary<Shared.Color, Brush> ColorToBrush)
        {
            bool flipped = false;
            List<TextBlock> numberTexts = new();

            // Making the board in UI
            Grid plankGrid = new()
            {
                Height = 200,
                Width = 100
            };
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
                rowRect.Tag = i;
                rowRect.MouseLeftButtonUp += (s, e) =>
                {
                    plankSectionClicked?.Invoke((int)((Rectangle)s).Tag);
                };
                TextBlock rowText = new()
                {
                    Text = Plank.plankScores[i].ToString(),
                    Tag = i,
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                numberTexts.Add(rowText);
                Grid.SetRow(rowRect, i);
                Grid.SetRow(rowText, i);
                Grid.SetColumnSpan(rowRect, 5);
                plankGrid.Children.Add(rowRect);
                plankGrid.Children.Add(rowText);
            }
            ;
            for (int i = 0; i < 4; i++)
            {
                plankGrid.ColumnDefinitions.Add(new() { Width = new GridLength(1, GridUnitType.Star) });
                Shared.Color color = (Shared.Color)i;
                int pos = 0;
                Rectangle pieceRect = new()
                {
                    Width = 15,
                    Height = 15,
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

                // Adding onclick functionality to the rectangles
                pieceRect.MouseLeftButtonUp += RectangleClick;

                Grid.SetColumn(pieceRect, i + 1);
                plankGrid.Children.Add(pieceRect);
            }

            // Making a flip button
            Button btn = new()
            {
                Content = "Flip board",
                Width = 60,
                Height = 30,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new(50),
            };
            btn.Click += (s, e) =>
            {
                flipped = !flipped;
                foreach (TextBlock block in numberTexts)
                {
                    block.Text = Plank.plankScores[4 - (int)block.Tag].ToString();
                    block.Tag = 4 - (int)block.Tag;
                }
            };
            TextBlock title = new()
            {
                Text = "Setup you board",
                Height = 50,
                Width = 200,
                VerticalAlignment = VerticalAlignment.Top,
                FontSize = 20,
                TextAlignment = TextAlignment.Center,
                Margin = new(10),
            };
            MainGrid.Children.Add(title);
            MainGrid.Children.Add(btn);
            MainGrid.Children.Add(plankGrid);

            return new(0, 0, 0, 0, flipped);
        }

        private async void RectangleClick(object sender, EventArgs e)
        {
            // Waiting until a plank is clicked
            TaskCompletionSource<int> tcs = new();

            void OnPlankSectionClicked(int n)
            {
                tcs.SetResult(n);
            }

            // Subscribing to the event
            plankSectionClicked += OnPlankSectionClicked;
            int num = await tcs.Task;
            // Unsubscribing from the event
            plankSectionClicked -= OnPlankSectionClicked;

            Grid.SetRow((Rectangle)sender, num);
        }
    }
}
