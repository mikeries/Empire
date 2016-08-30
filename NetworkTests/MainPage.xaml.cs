using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Sockets;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace NetworkTests
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Server server;
        public const string serverPort = "5555";
        private Client client;
        public const string clientPort = "5554";
        private PacketServer packetServer;
        public const string packetServerPort = "5556";
        private PacketClient packetClient;
        public const string packetClientPort = "5557";

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void startButton_Click(object sender, RoutedEventArgs e)
        {
            server = new Server(this);
            await server.StartListening(serverPort);

            client = new Client(this);
            await client.StartListening(clientPort);

            packetServer = new PacketServer(this);
            await packetServer.StartListening(packetServerPort);

            packetClient = new PacketClient(this);
            await packetClient.StartListening(packetClientPort);

            DisplayUpdate("All servers and clients are listening.");
        }

        private async void sendButton_Click(object sender, RoutedEventArgs e)
        {
            await client.Send(toBytes("Hello!"));
        }

        private async void connectButton_Click(object sender, RoutedEventArgs e)
        {
            await client.Connect("127.0.0.1", serverPort);
        }

        private async void waitResponseButton_Click(object sender, RoutedEventArgs e)
        {
            await client.ConnectAndWaitResponse("127.0.0.1", serverPort, toBytes("Waiting for reply"));
        }

        private async void packetConnectButton_Click(object sender, RoutedEventArgs e)
        {
            await packetClient.Connect("127.0.0.1", packetServerPort);
        }

        private async void packetSendButton_Click(object sender, RoutedEventArgs e)
        {
            await packetClient.Send(new SalutationPacket("Mike"));
        }

        private async void packetWaitResponseButton_Click(object sender, RoutedEventArgs e)
        {
            await packetClient.ConnectAndWaitResponse("127.0.0.1", packetServerPort, new SalutationPacket("Mike"));
        }

        public void NotifyUserFromAsyncThread(string strMessage)
        {
            var ignore = Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal, () => DisplayUpdate(strMessage));
        }

        private void DisplayUpdate(string update)
        {
            textBox.Content += Environment.NewLine + update;
        }

        public static byte[] toBytes(string message)
        {
            return Encoding.UTF8.GetBytes(message);
        }

        public static string toString(byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }


    }
}
