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
using Shared;

namespace SkullQueenClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LobbyView lobbyView;
        private GameView gameView;
        private bool GameStarted = false;
        public MainWindow()
        {
            InitializeComponent();
            lobbyView = new LobbyView();
            gameView = new GameView();

            lobbyView.StartGameClicked += StartGame;
            MainContent.Content = lobbyView;
        }
        public void StartGame(object sender, string playerName)
        {
            Player? player = ConnectToServer(playerName);
            lobbyView.AddPlayerToLobby(playerName);

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
                    if (GameStarted)
                    {
                        switch (message)
                        {
                            case string s when s.StartsWith("CARD")
                        }
                    }
                    else
                    {
                        switch (message)
                        {
                            case "GAME START":
                                MainContent.Content = gameView;
                                GameStarted = true;
                                break;
                            default:
                                // New player joined the lobby
                                lobbyView.AddPlayerToLobby(message);
                                break;
                        }
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