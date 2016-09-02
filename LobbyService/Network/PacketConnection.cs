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
        public delegate Task<NetworkPacket> TCPPacketCallback(StreamSocket socket, NetworkPacket packet);
        private TCPPacketCallback _TCPhandler;
        public delegate Task UDPPacketCallback(DatagramSocket socket, NetworkPacket packet);
        private UDPPacketCallback _UDPhandler;

        internal PacketConnection(ISerializer serializer)
        {
            _serializer = serializer;
            _networkConnection = new NetworkConnection();
        }

        internal void Close()
        {
            if(_serializer != null)
            {
                _serializer = null;
            }
            if(_networkConnection != null)
            {
                _networkConnection.Close();
            }
        }

        internal Task<StreamSocket> ConnectToTCP(string serverAddress, string serverPort)
        {
            return _networkConnection.ConnectToTCP(serverAddress,serverPort);
        }

        internal Task StartTCPListener(string port, TCPPacketCallback handler)
        {
            _TCPhandler = handler;
            return _networkConnection.StartTCPListener(port, TCPPacketHandler);
        }

        private async Task<byte[]> TCPPacketHandler(StreamSocket socket, byte[] data)
        {
            try
            {
                NetworkPacket packet = _serializer.ConstructPacketFromMessage(data);
                NetworkPacket reply = await _TCPhandler(socket, packet);
                return _serializer.CreateMessageFromPacket(reply);
            }
            catch { }
            return new byte[0];
        }

        internal Task SendTCPData(StreamSocket socket, NetworkPacket packet)
        {
            try
            {
                byte[] data = _serializer.CreateMessageFromPacket(packet);
                return _networkConnection.sendTCPData(socket, data);
            } catch { }
            return Task.Delay(0);
        }

        internal async Task<NetworkPacket> ConnectAndWaitResponse(string address, string port, NetworkPacket packet)
        {
            try
            {
                byte[] data = _serializer.CreateMessageFromPacket(packet);
                byte[] response = await _networkConnection.ConnectAndWaitResponse(address, port, data);
                return _serializer.ConstructPacketFromMessage(response);
            }
            catch { }
            return new AcknowledgePacket();
        }

        internal Task StartUDPListener(string port, UDPPacketCallback handler)
        {
            _UDPhandler = handler;
            return _networkConnection.StartUDPListener(port, UDPPacketHandler);
        }

        internal Task SendUDPData(string address, string port, NetworkPacket packet)
        {
            try
            {
                byte[] data = _serializer.CreateMessageFromPacket(packet);
                return _networkConnection.SendUDPData(address, port, data);
            }
            catch { }
            return Task.Delay(0);
        }

        private Task UDPPacketHandler(DatagramSocket socket, byte[] data)
        {
            try
            {
                NetworkPacket packet = _serializer.ConstructPacketFromMessage(data);
                _UDPhandler(socket, packet);
            }
            catch { }
            return Task.Delay(0);
        }
    }
}