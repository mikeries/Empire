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
    class PacketServer
    {
        private PacketConnection _connection;
        private Dictionary<string,StreamSocket> clientList = new Dictionary<string, StreamSocket>();  // dictionary of StreamSockets back to the various clients
        private MainPage _rootPage;

        public PacketServer(MainPage rootPage)
        {
            _connection = new PacketConnection(new EmpireSerializer());
            _rootPage = rootPage;
        }

        public async Task StartListening(string port)
        {
            await _connection.StartTCPListener(port, handler);
        }

        public async Task Connect(string address, string port)
        {
            await _connection.ConnectToTCP(address, port);
            _rootPage.NotifyUserFromAsyncThread("Packet server is connected.");
        }

        public async Task<NetworkPacket> handler(StreamSocket socket, NetworkPacket packet)
        {
            string address = socket.Information.RemoteAddress.DisplayName;

            if(!clientList.ContainsKey(address))
            {
                StreamSocket clientSocket = await _connection.ConnectToTCP(address, MainPage.clientPort);
                _rootPage.NotifyUserFromAsyncThread("Packet server is connected.");
                clientList.Add(address, clientSocket);
            }

            _rootPage.NotifyUserFromAsyncThread("Packet server received: " + packet.Type);
            await Task.Delay(5000);

            return new AcknowledgePacket();
        }

    }
}
