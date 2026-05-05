using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SkullQueenClient
{
    /// <summary>
    /// Interaction logic for EndScreen.xaml
    /// </summary>
    /// 
    public partial class EndScreen : UserControl
    {
        public EndScreen(int score, Dictionary<Opponent, int> opponentScores)
        {
            InitializeComponent();
            SetScore(score, opponentScores);
        }
        public void SetScore(int score, Dictionary<Opponent, int> opponentScores)
        {
            // Calculating the scores and places
            int placing = 1;
            List<Opponent> opponentPlaces = opponentScores.Keys.OrderBy(opp => opponentScores[opp]).ToList();
            for (int i = 0; i < opponentPlaces.Count -1; i++)
            {
                if (score < opponentScores[opponentPlaces[i]] && score > opponentScores[opponentPlaces[i + 1]])
                {
                    // 1 to negate the i = 0, 1 for the next pos over of i
                    placing = i + 1 + 1;
                }
            }
            ScoreText.Text = $"You scored {score}\nThis caused you to place {placing}" + (score == 1 ? "st" : "nd");

            // Displaying the opponents
            for (int i = 0; i < opponentPlaces.Count; i++)
            {
                Opponent opp = opponentPlaces[i];
                int oppScore = opponentScores[opp];
                if (i + 1 == placing)
                {
                    // Adding the player
                    ScoringList.Items.Add(new { Place = placing, Name = "You", Score = score });
                }
                ScoringList.Items.Add(new { Place = i + 1 >= placing ? i + 1 : i + 2, Name = opp.name, Score = oppScore });
            }
        }
    }
}
