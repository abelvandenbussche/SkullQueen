using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Sockets;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace SkullQueen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MenuWindow : Window
    {
        Game? game;
        Player thisPlayer;
        String address = "127.0.0.1";
        
        public MenuWindow()
        {
            InitializeComponent();

            // getting button input
            HostButton.Click += HostButtonClick;
            JoinButton.Click += JoinButtonClick;

            StartButton.Click += StartGame;
        }
        public void HostButtonClick(object sender, RoutedEventArgs e)
        {
            game = new();
            game.UpdateLobbyText += HandleLobbyTextUpdate;
            this.thisPlayer = game.Host(NameField, StartButton);

            HideStuff();
        }
        public async void JoinButtonClick(object sender, RoutedEventArgs e)
        {
            // starting a client
            TcpClient tcpClient = new(address, 5050);

            // creating a player
            thisPlayer = new(NameField.Text, tcpClient);

            thisPlayer.SendTcpData(thisPlayer.name);

            HideStuff();

            // getting messages from the host
            await Task.Run(() =>
            {
                while (true)
                {
                    string data = thisPlayer.ReceiveTcpData();
                    if (data == "GAME START")
                    {
                        Debug.WriteLine("The game would start");
                        break;
                    }
                    Debug.WriteLine(data);
                    // otherwise updating the lobby text
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        LobbyTextBlock.Text += data + Environment.NewLine;
                    });
                }
            });
        }

        private void HideStuff()
        {
            HostButton.Visibility = Visibility.Collapsed;
            JoinButton.Visibility = Visibility.Collapsed;
            NameField.Visibility = Visibility.Collapsed;
            NameLabel.Visibility = Visibility.Collapsed;
        }

        private void StartGame(Object sender, EventArgs args)
        {
            if (game != null)
            {
                // broadcasting the start of the game
                game.Broadcast("GAME START");

                // creating a new window
                // making it the main one
                // closing this one
                MainWindow window = new(thisPlayer, game);
                window.Show();
                Application.Current.MainWindow = window;
                this.Close();
            }
        }
        private void HandleLobbyTextUpdate(object sender, string toAdd)
        {
            LobbyTextBlock.Text += toAdd;
            // sending a message to all people
            game.Broadcast(toAdd);
        }
    }
}