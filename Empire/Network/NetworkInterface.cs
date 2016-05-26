using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EmpireUWP.Network
{
    internal static class NetworkInterface
    {
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const string hostName = "Mike-PC";
        private const string hostAddress = "ries.asuscomm.com";
        private const int _hostPort = 5394;
        private const int MaximumPacketSize = 1472;

        private static UdpClient _socket;
        public static IPEndPoint HostEndPoint;
        private static IPEndPoint _clientEndPoint;

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
            }
            catch (SocketException e)
            {
                if (e.ErrorCode == (int)SocketError.AddressAlreadyInUse)
                {
                    _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    _port = _clientEndPoint.Port;
                    _socket = new UdpClient(_clientEndPoint);
                }
                else
                {
                    log.Error("Unexpected network exception during initialization.", e);
                    throw (e);
                }
            }

            _socket.BeginReceive(new AsyncCallback(ReceiveData), _socket);
        }

        private static void ReceiveData(IAsyncResult result)
        {
            try
            {
                _socket = result.AsyncState as UdpClient;
                IPEndPoint source = new IPEndPoint(IPAddress.Any, 0);
                byte[] message = _socket.EndReceive(result, ref source);

                NetworkPacket packet = NetworkPacket.ConstructPacketFromMessage(message);
                OnPacketReceived(source, packet);

                _socket.BeginReceive(new AsyncCallback(ReceiveData), _socket);
            }
            catch (SocketException e)
            {
                if (e.ErrorCode == (int)SocketError.ConnectionReset)
                {
                    log.Warn("Connection closed by host.",e);
                    // TODO: terminate network connection and inform game.
                }
                {
                    log.Fatal("Fatal network error", e);
                    
                    throw e;
                }
            }
        }

        private static void SendDataToClient(IPEndPoint endpoint, byte[] message)
        {
            if (message.Length > MaximumPacketSize)
            {
                log.Warn("Packet size exceeds maximum. (" + message.Length+")");
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
