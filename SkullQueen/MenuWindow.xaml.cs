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

namespace SkullQueen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MenuWindow : Window
    {
        Game? game;
        String address = "127.0.0.1";
        
        public MenuWindow()
        {
            InitializeComponent();

            // getting button input
            HostButton.Click += HostButtonClick;
            JoinButton.Click += JoinButtonClick;
        }
        public void HostButtonClick(object sender, RoutedEventArgs e)
        {
            game = new();
            game.Host(NameField, LobbyTextBlock, StartButton);
        }
        public void JoinButtonClick(object sender, RoutedEventArgs e)
        {
            // starting a client
            TcpClient tcpClient = new(address, 5050);
            Stream stream = tcpClient.GetStream();
            // sending a test message

        }
    }
}