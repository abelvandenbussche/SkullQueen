using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
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
        private event Action<int>? PlankSectionClicked;
        private event Action<List<Rectangle>, bool>? ButtonClicked;
        public event Action? HandUpdated;
        public bool classicCards = false;
        Grid containmentGrid = new()
        {
            Margin = new(0, 50, 0, 100),
        };
        
        public GameView()
        {
            InitializeComponent();
            containmentGrid.RowDefinitions.Add(new() { Height = new(1, GridUnitType.Auto) });
            containmentGrid.RowDefinitions.Add(new() { Height = new(1, GridUnitType.Auto) });
            containmentGrid.RowDefinitions.Add(new() { Height = new(1, GridUnitType.Auto) });
        }
        private void MakePlank(Dictionary<Shared.Color, Brush> ColorToBrush)
        {
            bool flipped = false;
            List<Rectangle> pieces = new();
            List<TextBlock> numberTexts = new();

            // Making the board in UI
            Grid plankGrid = new()
            {
                Height =  160,
                Width = 80,
                Margin = new(10)
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
                    PlankSectionClicked?.Invoke((int)((Rectangle)s).Tag);
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
                    Width = 12,
                    Height = 12,
                    Margin = new Thickness(2, 0, 2, 0),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Tag = color,
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
                pieces.Add(pieceRect);
            }

            // Making a flip button
            Button flipButton = new()
            {
                Content = "Flip board",
                Width = 60,
                Height = 30,
            };
            flipButton.Click += (s, e) =>
            {
                flipped = !flipped;
                foreach (TextBlock block in numberTexts)
                {
                    block.Text = Plank.plankScores[4 - (int)block.Tag].ToString();
                    block.Tag = 4 - (int)block.Tag;
                }
            };
            Button finnishButton = new()
            {
                Height = 30,
                Width = 60,
                Content = "Ready",
            };
            finnishButton.Click += (s, e) => ButtonClicked?.Invoke(pieces, flipped);

            Grid.SetRow(flipButton, 0);
            Grid.SetRow(finnishButton, 1);
            Grid.SetRow(plankGrid, 2);

            containmentGrid.Children.Add(finnishButton);
            containmentGrid.Children.Add(flipButton);
            containmentGrid.Children.Add(plankGrid);

            MainGrid.Children.Add(containmentGrid);
        }
        public async Task<Plank> GetPlank(Dictionary<Shared.Color, Brush> ColorToBrush)
        {
            TaskCompletionSource<Plank> tcs = new();
            void OnFinnish(List<Rectangle> pieces, bool flipped)
            {
                int redPos = Grid.GetRow(pieces.Find(rect => (Shared.Color)rect.Tag == Shared.Color.Red));
                int greenPos = Grid.GetRow(pieces.Find(rect => (Shared.Color)rect.Tag == Shared.Color.Green));
                int yellowPos = Grid.GetRow(pieces.Find(rect => (Shared.Color)rect.Tag == Shared.Color.Yellow));
                int bluePos = Grid.GetRow(pieces.Find(rect => (Shared.Color)rect.Tag == Shared.Color.Blue));

                tcs.SetResult(new(redPos, greenPos, yellowPos, bluePos, flipped));
            }
            StatusText.Text = "Setup you plank based on the cards you have\nPress ready when finished";
            MakePlank(ColorToBrush);

            ButtonClicked += OnFinnish;
            Plank plank = await tcs.Task;
            ButtonClicked -= OnFinnish;

            // Clearing the UI
            MainGrid.Children.Remove(containmentGrid);
            StatusText.Text = "Waiting on other players";
            return plank;
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
            PlankSectionClicked += OnPlankSectionClicked;
            int num = await tcs.Task;
            // Unsubscribing from the event
            PlankSectionClicked -= OnPlankSectionClicked;

            Grid.SetRow((Rectangle)sender, num);
        }
        private void RadioButton_Checked(object sender,  RoutedEventArgs e)
        {
            RadioButton selected = (RadioButton)sender;
            classicCards = selected == ClassicRadioButton;
            HandUpdated?.Invoke();
        }
    }
}
