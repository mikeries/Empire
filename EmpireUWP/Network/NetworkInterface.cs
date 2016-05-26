using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace EmpireUWP.Network
{
    internal static class NetworkInterface
    {
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger("NetworkInterface");

        private const string hostName = "Mike-PC";
        private const string hostAddress = "ries.asuscomm.com";
        private const int _hostPort = 5394;
        private const int MaximumPacketSize = 1472;

        private static MessageWebSocket _socket;
        private static DataWriter _writer;
        public static IPEndPoint HostEndPoint;
        private static IPEndPoint _clientEndPoint;

        private static int _port = _hostPort;             // default to using the same port as the host

        public static bool IsHost = false;

        static NetworkInterface() { }

        public async static void Initialize()
        {
            try
            {
                Uri server = new Uri("localhost:1967");
                _socket = new MessageWebSocket();
                _socket.Control.MessageType = SocketMessageType.Binary;
                _socket.MessageReceived += MessageReceived;

                await _socket.ConnectAsync(server);
                _writer = new DataWriter(_socket.OutputStream);

                if (server.DnsSafeHost == hostName)
                {
                    IsHost = true;
                }
            }
            catch (SocketException e)
            {
                // TODO:
            }

            //_socket.BeginReceive(new AsyncCallback(ReceiveData), _socket);
        }

        private static void MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            //try
            //{
            //    using (DataReader reader = args.GetDataReader())
            //    {
            //        reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
            //        string read = reader.ReadString(reader.UnconsumedBufferLength);
            //    }

            //    NetworkPacket packet = (NetworkPacket)ConstructPacketFromMessage(message);
            //    OnPacketReceived(source, packet);

            //    _socket.BeginReceive(new AsyncCallback(ReceiveData), _socket);
            //}
            //catch (SocketException e)
            //{
            //    if (e.ErrorCode == (int)SocketError.ConnectionReset)
            //    {
            //        log.Warn("Connection closed by host.",e);
            //        // TODO: terminate network connection and inform game.
            //    }
            //    {
            //        log.Fatal("Fatal network error", e);
                    
            //        throw e;
            //    }
            //}
        }

        private static void SendDataToClient(IPEndPoint endpoint, byte[] message)
        {
            //if (message.Length > MaximumPacketSize)
            //{
            //    log.Warn("Packet size exceeds maximum. (" + message.Length+")");
            //}
            //_socket.Send(message, message.Length, endpoint);
        }

        public static void SendPacket(IPEndPoint endpoint, NetworkPacket packet)
        {
            byte[] message = CreateMessageFromPacket(packet);
            SendDataToClient(endpoint, message);
        }

        private static byte[] CreateMessageFromPacket(NetworkPacket packet)
        {
            //TODO:  Figure out a better way to handle the list of possible packet types
            byte[] message = null;
            DataContractSerializer serializer = new DataContractSerializer(packet.GetType(), new Type[]
                {
                    typeof(NetworkPacket),
                    typeof(AcknowledgePacket),
                    typeof(EntityPacket),
                    typeof(PingPacket),
                    typeof(PlayerData),
                    typeof(SalutationPacket),
                    typeof(ShipCommand)
                });

            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    serializer.WriteObject(stream, packet);
                    message = stream.ToArray();
                }
            }
            catch (SerializationException e)
            {
                log.Fatal("Serialization exception", e);
                throw new Exception(e.Message);
            }

            return message;
        }

        private static NetworkPacket ConstructPacketFromMessage(byte[] message)
        {
            NetworkPacket packet = null;
            DataContractSerializer serializer = new DataContractSerializer(typeof(NetworkPacket));

            try
            {
                using (MemoryStream stream = new MemoryStream(message))
                {
                    packet = (NetworkPacket)serializer.ReadObject(stream);
                }
            }
            catch (Exception e)
            {
                log.Fatal("Deserialization exception", e);
            }

            return packet;
        }


        public static event EventHandler<PacketReceivedEventArgs> PacketReceived = delegate { };
        private static void OnPacketReceived(IPEndPoint source, NetworkPacket packet)
        {
            PacketReceived?.Invoke(null, new PacketReceivedEventArgs(source, packet));
        }
    }
}
