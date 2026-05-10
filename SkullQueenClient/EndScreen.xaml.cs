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
            List<Opponent> opponentPlaces = opponentScores.Keys.OrderBy(opp => opponentScores[opp]).Reverse().ToList();

            // Determining the player's place
            int placing = opponentPlaces.Count + 1;
            for (int i = 0; i < opponentPlaces.Count; i++)
            {
                if (score >= opponentScores[opponentPlaces[i]])
                {
                    placing = i + 1;
                    break;
                }
            }

            // Displaying the ranking
            for (int i = 0; i < opponentPlaces.Count; i++)
            {
                Opponent opp = opponentPlaces[i];
                int oppScore = opponentScores[opp];
                if (i + 1 == placing)
                {
                    // Adding the player
                    ScoringList.Items.Add(new { Place = placing, Name = "You", Score = score });
                }
                ScoringList.Items.Add(new { Place = i + 1 >= placing ? i + 1 : i + 1, Name = opp.name, Score = oppScore });
            }
            // If the player is last, add them to the end of the list
            if (placing == opponentPlaces.Count + 1)
            {
                ScoringList.Items.Add(new { Place = placing, Name = "You", Score = score });
            }
        }
    }
}
