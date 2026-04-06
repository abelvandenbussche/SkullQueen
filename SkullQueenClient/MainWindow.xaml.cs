using System.Net.Sockets;
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

namespace SkullQueenClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LobbyView lobbyView = new LobbyView();
            GameView gameView = new GameView();
            MainContent.Content = lobbyView;

            Player? player = ConnectToServer("Player1");
            lobbyView.AddPlayerToLobby("Player1");

            if (player == null)
            {
                // Connection failure, close the application
                return;
            }

            // Start listening for messages from the server
            Task listener = player.ListenForMessages(message =>
            {
                // Update the UI with the received message
                Dispatcher.Invoke(() =>
                {
                    switch (message)
                    {
                        case "GAME START":
                            MainContent.Content = gameView;
                            break;
                        default:
                            // New player joined the lobby
                            lobbyView.AddPlayerToLobby(message);
                            break;
                    }
                });
            });
        }
        public Player? ConnectToServer(string playerName)
        {
            try
            {
                TcpClient client = new TcpClient("localhost", 5050);
                Player player = new(playerName, client);
                player.SendMessage(playerName);
                return player;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error connecting to server: " + ex.Message);
                return null;
            }
        }
    }
}