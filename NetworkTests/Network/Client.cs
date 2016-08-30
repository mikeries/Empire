using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;

namespace NetworkTests
{
    class Client
    {
        private NetworkConnection _connection;
        private StreamSocket _serverSocket;  // connection to the server
        private MainPage _rootPage;

        public Client(MainPage root)
        {
            _rootPage = root;
            _connection = new NetworkConnection();
        }

        public Task StartListening(string port)
        {
            return _connection.StartTCPListener(port, dataHandler);
        }

        public async Task Connect(string address, string port)
        {
            _serverSocket = await _connection.ConnectToTCP(address, port);
            _rootPage.NotifyUserFromAsyncThread("Client is connected.");
        }

        public Task Send(byte[] data)
        {
            _rootPage.NotifyUserFromAsyncThread("Client is sending: " + MainPage.toString(data));
            return _connection.sendTCPData(_serverSocket, data);
        }

        public async Task ConnectAndWaitResponse(string address, string port, byte[] data)
        {
            _rootPage.NotifyUserFromAsyncThread("Client is sending: " + MainPage.toString(data));
            byte[] reply = await _connection.ConnectAndWaitResponse(address, port, data);
            _rootPage.NotifyUserFromAsyncThread("Server replied with: " + MainPage.toString(reply));
        }

        public async Task<byte[]> dataHandler(StreamSocket socket, byte[] data)
        {
            if (_serverSocket == null)
            {
                string address = socket.Information.RemoteAddress.DisplayName;
                await Connect(address, MainPage.serverPort);
            }
            return MainPage.toBytes("Hi");
        }
    }
}
