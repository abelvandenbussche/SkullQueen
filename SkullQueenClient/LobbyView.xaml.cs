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
using System.Text.RegularExpressions;

namespace SkullQueenClient
{
    /// <summary>
    /// Interaction logic for LobbyView.xaml
    /// </summary>
    public partial class LobbyView : UserControl
    {
        public event EventHandler<string>? StartGameClicked;
        public event Action ReadyUpClicked;
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
            string name = UsernameTextBox.Text;
            // Replacing whitespace musing regex
            name = Regex.Replace(name, @"\s+", "_");

            StartGameClicked?.Invoke(this, name);

            // hiding the button and text box after clicking start game
            Button button = sender as Button;
            button.IsEnabled = false;
            button.Visibility = Visibility.Collapsed;

            UsernameTextBox.IsEnabled = false;
            UsernameTextBox.Visibility = Visibility.Collapsed;

            UsernameHeader.Visibility = Visibility.Collapsed;

            ReadyUpButton.Visibility = Visibility.Visible;
        }
        private void ReadyUpButton_Click(object sender, RoutedEventArgs e)
        {
            // Sending a message to the server
            ReadyUpButton.IsEnabled = false;
            ReadyUpClicked?.Invoke();
        }
    }
}
