using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Empire.Network
{
    internal static class NetworkInterface
    {
        private const string hostName = "Mike-PC";
        private const string hostAddress = "ries.asuscomm.com";
        private const int _hostPort = 5394;
        private const int MaximumPacketSize = 1472;

        private static UdpClient _socket;
        public static IPEndPoint HostEndPoint;
        private static IPEndPoint _clientEndPoint;
        public static string ConnectionID { get; private set; }

        private static int _port = _hostPort;             // default to using the same port as the host

        public static bool IsHost = false;

        static NetworkInterface() { }

        //TODO:  Accept host machine, host address, and host port as initialization parms
        public static void Initialize()
        {
            HostEndPoint = new IPEndPoint(IPAddress.Parse(Dns.GetHostEntry(hostAddress).AddressList[0].ToString()), _hostPort);
            try
            {
                _socket = new UdpClient(_hostPort);
                if (Dns.GetHostName() == hostName)
                {
                    IsHost = true;
                }
                ConnectionID = HostEndPoint.ToString();
            }
            catch (SocketException e)
            {
                if (e.ErrorCode == (int)SocketError.AddressAlreadyInUse)
                {
                    _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    _port = _clientEndPoint.Port;
                    _socket = new UdpClient(_clientEndPoint);
                    ConnectionID = _clientEndPoint.ToString();
                }
                else
                {
                    throw (e);
                }
            }

            _socket.BeginReceive(new AsyncCallback(ReceiveData), _socket);
        }

        private static void ReceiveData(IAsyncResult result)
        {
            _socket = result.AsyncState as UdpClient;
            IPEndPoint source = new IPEndPoint(IPAddress.Any, 0);
            byte[] message = _socket.EndReceive(result, ref source);

            NetworkPacket packet = NetworkPacket.ConstructPacketFromMessage(message);
            OnPacketReceived(source, packet);

            _socket.BeginReceive(new AsyncCallback(ReceiveData), _socket);
        }

        private static void SendDataToClient(IPEndPoint endpoint, byte[] message)
        {
            if (message.Length > MaximumPacketSize)
            {
                // TODO: log this for now...
            }
            _socket.Send(message, message.Length, endpoint);
        }

        public static void SendPacket(IPEndPoint endpoint, NetworkPacket packet)
        {
            byte[] message = packet.CreateMessageFromPacket();
            SendDataToClient(endpoint, message);
        }

        public static event EventHandler<PacketReceivedEventArgs> PacketReceived = delegate { };
        private static void OnPacketReceived(IPEndPoint source, NetworkPacket packet)
        {
            PacketReceived?.Invoke(null, new PacketReceivedEventArgs(source, packet));
        }
    }
}
