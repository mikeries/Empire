using Windows.Networking;
using Windows.Networking.Sockets;
using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.Storage.Streams;

namespace LobbyService
{
    internal class PacketConnection
    {
        private ISerializer _serializer;
        private NetworkConnection _networkConnection;
        public delegate Task<NetworkPacket> PacketCallback(StreamSocket socket, NetworkPacket packet);
        private PacketCallback _handler;

        internal PacketConnection(ISerializer serializer)
        {
            _serializer = serializer;
            _networkConnection = new NetworkConnection();
        }

        internal Task<StreamSocket> ConnectToTCP(string serverAddress, string serverPort)
        {
            return _networkConnection.ConnectToTCP(serverAddress,serverPort);
        }

        internal Task StartTCPListener(string port, PacketCallback handler)
        {
            _handler = handler;
            return _networkConnection.StartTCPListener(port, packetHandler);
        }

        private async Task<byte[]> packetHandler(StreamSocket socket, byte[] data)
        {
            NetworkPacket packet = _serializer.ConstructPacketFromMessage(data);
            NetworkPacket reply = await _handler(socket, packet);
            return _serializer.CreateMessageFromPacket(reply);
        }

        internal Task SendTCPData(StreamSocket socket, NetworkPacket packet)
        {
            byte[] data = _serializer.CreateMessageFromPacket(packet);
            return _networkConnection.sendTCPData(socket, data);
        }

        internal async Task<NetworkPacket> ConnectAndWaitResponse(string address, string port, NetworkPacket packet)
        {
            byte[] data = _serializer.CreateMessageFromPacket(packet);
            byte[] response = await _networkConnection.ConnectAndWaitResponse(address, port, data);
            return _serializer.ConstructPacketFromMessage(response);
        }

    }
}