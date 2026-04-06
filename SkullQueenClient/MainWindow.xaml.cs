using System.ComponentModel.DataAnnotations;
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
using SkullQueenServer;

namespace SkullQueenClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LobbyView lobbyView;
        private GameView gameView;
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
                // splitting the message
                // trying to parse the command
                object cmd = null;
                try
                {
                    string commandStr = message.Split(' ')[0];
                    cmd = Enum.Parse(typeof(Command), commandStr);
                    cmd = cmd as Command? ?? throw new Exception("Invalid command");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error parsing message: " + ex.Message);
                    return;
                }
                string[] args = message.Split(' ').Skip(1).ToArray();

                if (cmd == null)
                {
                    return;
                }

                switch (cmd)
                {
                    case Command.StartGame:
                        // Switch to the game view
                        Dispatcher.Invoke(() =>
                        {
                            MainContent.Content = gameView;
                        });
                        break;
                    case Command.PlayCard:
                        SkullQueenServer.Color suit = (SkullQueenServer.Color)Enum.Parse(typeof(SkullQueenServer.Color), args[0]);
                        int rank = int.Parse(args[1]);
                        Card newCard = new(suit, rank);
                        break;
                    case Command.JoinLobby:
                        lobbyView.AddPlayerToLobby(args[0]);
                        break;
                }

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