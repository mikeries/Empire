using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NetworkTests
{
    class Server
    {
        private NetworkConnection _connection;
        private Dictionary<string,StreamSocket> clientList = new Dictionary<string, StreamSocket>();  
        private MainPage _rootPage;

        public Server(MainPage rootPage)
        {
            _connection = new NetworkConnection();
            _rootPage = rootPage;
        }

        public async Task StartListening(string port)
        {
            await _connection.StartTCPListener(port, tcpHandler);
            await _connection.StartUDPListener(port, udpHandler);
        }

        public async Task Connect(string address, string port)
        {
            await _connection.ConnectToTCP(address, port);
            _rootPage.NotifyUserFromAsyncThread("Server is connected.");
        }

        public async Task<byte[]> tcpHandler(StreamSocket socket, byte[] data)
        {
            string address = socket.Information.RemoteAddress.DisplayName;

            if(!clientList.ContainsKey(address))
            {
                StreamSocket clientSocket = await _connection.ConnectToTCP(address, MainPage.clientPort);

                _rootPage.NotifyUserFromAsyncThread("Server is connected.");
                clientList.Add(address, clientSocket);
            }

            _rootPage.NotifyUserFromAsyncThread("Server received: " + MainPage.toString(data));

            await Task.Delay(5000);
            return MainPage.toBytes("Hi");
        }

        public void udpHandler(DatagramSocket socket, byte[] data)
        {
            _rootPage.NotifyUserFromAsyncThread("Server received UDP: " + MainPage.toString(data));

        }

    }
}
