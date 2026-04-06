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
    /// Interaction logic for LobbyView.xaml
    /// </summary>
    public partial class LobbyView : UserControl
    {
        public event EventHandler<string> StartGameClicked;
        public LobbyView()
        {
            InitializeComponent();
        }
        public void AddPlayerToLobby(string playerName)
        {
            LobbyList.Items.Add(playerName);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (UsernameTextBox.Text.Length == 0)
            {
                MessageBox.Show("Please enter a username.");
                return;
            }
            StartGameClicked?.Invoke(this, UsernameTextBox.Text);

            // hiding the button and text box after clicking start game
            Button button = sender as Button;
            button.IsEnabled = false;
            button.Visibility = Visibility.Collapsed;

            UsernameTextBox.IsEnabled = false;
            UsernameTextBox.Visibility = Visibility.Collapsed;

            UsernameHeader.Visibility = Visibility.Collapsed;
        }
    }
}
