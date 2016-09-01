using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;

namespace NetworkTests
{
    class PacketClient
    {
        private PacketConnection _connection;
        private StreamSocket _serverSocket;  // connection to the server
        private MainPage _rootPage;

        public PacketClient(MainPage root)
        {
            _rootPage = root;
            _connection = new PacketConnection(new EmpireSerializer());
        }

        public Task StartListening(string port)
        {
            return _connection.StartTCPListener(port, dataHandler);
        }

        public async Task Connect(string address, string port)
        {
            _serverSocket = await _connection.ConnectToTCP(address, port);
            _rootPage.NotifyUserFromAsyncThread("Packet client is connected.");
        }

        public Task Send(string address, string port, NetworkPacket packet)
        {
            _rootPage.NotifyUserFromAsyncThread("Packet client is sending via UDP: " + packet.Type);
            return _connection.SendUDPData(address, port, packet);
        }

        public async Task ConnectAndWaitResponse(string address, string port, NetworkPacket packet)
        {
            _rootPage.NotifyUserFromAsyncThread("Packet client is sending: " + packet.Type);
            NetworkPacket reply = await _connection.ConnectAndWaitResponse(address, port, packet);
            _rootPage.NotifyUserFromAsyncThread("Packet server replied with: " + reply.Type);
        }

        public async Task<NetworkPacket> dataHandler(StreamSocket socket, NetworkPacket packet)
        {
            if (_serverSocket == null)
            {
                string address = socket.Information.RemoteAddress.DisplayName;
                await Connect(address, MainPage.serverPort);
            }
            return new AcknowledgePacket();
        }
    }
}
