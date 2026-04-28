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
using System.Diagnostics;
using System.DirectoryServices;

namespace SkullQueenClient
{
    /// <summary>
    /// Interaction logic for LobbyView.xaml
    /// </summary>
    public partial class LobbyView : UserControl
    {
        public event Action<string, string>? StartGameClicked;
        public event Action<string>? BotDifficultyChanged;
        public event Action? ReadyUpClicked;
        public event Action? AddBot;
        public event Action? RemoveBot;
        public LobbyView()
        {
            InitializeComponent();
        }
        public void AddPlayerToLobby(string playerName)
        {
            LobbyList.Items.Add(playerName);
        }
        public void RemovePlayerFromLobby(string playerName)
        {
            for (int i = 0;  i < LobbyList.Items.Count; i++)
            {
                if (LobbyList.Items[i].ToString() == playerName)
                {
                    LobbyList.Items.RemoveAt(i);
                    break;
                }
            }
        }

        private void FindGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsernameTextBox.Text.Length == 0)
            {
                MessageBox.Show("Please enter a username.");
                return;
            }
            string name = UsernameTextBox.Text;
            string lobbyCode = LobbyCodeTextBox.Text;
            // Replacing whitespace musing regex
            name = Regex.Replace(name, @"\s+", "_");

            StartGameClicked?.Invoke(name, lobbyCode);

            // hiding the button and text box after clicking start game
            Button button = (sender as Button)!;
            button.IsEnabled = false;
            button.Visibility = Visibility.Collapsed;

            // Making elements invisible that should not be visible in the lobby
            UsernameTextBox.IsEnabled = false;
            UsernameTextBox.Visibility = Visibility.Collapsed;
            UsernameHeader.Visibility = Visibility.Collapsed;
            LobbyCodeTextBox.Visibility = Visibility.Collapsed;
            LobbyCodeFieldText.Visibility = Visibility.Collapsed;

            // Making elements visible that should only be visible in the lobby
            ReadyUpButton.Visibility = Visibility.Visible;
            BotButton.Visibility = Visibility.Visible;
            BotRemovalButton.Visibility = Visibility.Visible;
            DifficultyButtonGrid.Visibility = Visibility.Visible;
            LobbyCodeText.Visibility = Visibility.Visible;
        }
        private void ReadyUpButton_Click(object sender, RoutedEventArgs e)
        {
            // Sending a message to the server
            ReadyUpButton.IsEnabled = false;
            ReadyUpClicked?.Invoke();
        }
        private void BotButton_Click(object sender, RoutedEventArgs e)
        {
            AddBot?.Invoke();
        }
        private void BotRemovalButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveBot?.Invoke();
        }
        public void ChangeDifficulty(string difficulty)
        {
            switch (difficulty)
            {
                case "Easy":
                    EasyBotRadioButton.IsChecked = true;
                    break;
                case "Medium":
                    MediumBotRadioButton.IsChecked = true;
                    break;
                case "Hard":
                    HardBotRadioButton.IsChecked = true;
                    break;
            }
        }
        private void BotDifficultyRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            // Defaults to easy if for some reason the name of the button is not recognized
            string diff = "Easy";
            RadioButton button = (sender as RadioButton)!;
            switch (button.Name)
            {
                case "EasyBotRadioButton":
                    diff = "Easy";
                    break;

                case "MediumBotRadioButton":
                    diff = "Medium";
                    break;

                case "HardBotRadioButton":
                    diff = "Hard";
                    break;
            }
            BotDifficultyChanged?.Invoke(diff);
        }
        public void SetLobbyCode(string code)
        {
            Debug.WriteLine("Setting lobby code to " + code);
            LobbyCodeText.Text = "LobbyCode:  " + code;
        }
    }
}
