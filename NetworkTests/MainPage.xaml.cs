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
        private const string serverPort = "5555";
        private Client client;
        private const string clientPort = "5554";

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

            DisplayUpdate("Server and client are listening.");
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
            await client.WaitResponse(toBytes("Waiting for reply"));
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
            return Encoding.ASCII.GetBytes(message);
        }

        public static string toString(byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }

    }
}
