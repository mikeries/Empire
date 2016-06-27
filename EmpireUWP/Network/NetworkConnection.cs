using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace EmpireUWP.Network
{
    internal class NetworkConnection
    {
        private StreamSocket _hostSocket = null;
        private StreamSocketListener _listener = null;
        private static Type[] _knownTypes = {
            typeof(AcknowledgePacket),
            typeof(EntityPacket),
            typeof(NetworkPacket),
            typeof(PingPacket),
            typeof(PlayerData),
            typeof(SalutationPacket),
            typeof(ShipCommand),
        };
        public bool Connected { get; private set; }

        public NetworkConnection() {}

        public async Task ConnectAsync(string address, string port)
        {
            if (Connected)
            {
                return;
            }

            try
            {
                _hostSocket = new StreamSocket();
                HostName hostName = new HostName(address);
                await _hostSocket.ConnectAsync(hostName, port);
                Connected = true;
            }
            catch (Exception e)
            {
                if (SocketError.GetStatus(e.HResult) == SocketErrorStatus.Unknown)
                {
                    throw;
                }
            }
        }

        public void SendPacketToHost(NetworkPacket packet)
        {
                SendPacketTo(_hostSocket, packet);
        }

        public void SendPacketTo(StreamSocket destinationSocket, NetworkPacket packet)
        {
            if (Connected)
            {
                byte[] message = CreateMessageFromPacket(packet);
                SendTo(destinationSocket, message);
            }
        }

        public void SendPacketToAll(List<StreamSocket> destinationSockets, NetworkPacket packet)
        {
            byte[] message = CreateMessageFromPacket(packet);
            SendToAll(destinationSockets, message);
        }

        private async void SendTo(StreamSocket destinationSocket, byte[] message)
        {
            if (destinationSocket==null)
            {
                return;
            }

            DataWriter writer = new DataWriter(destinationSocket.OutputStream);

            writer.WriteUInt32((UInt32)message.Length);
            writer.WriteBytes(message);

            try
            {
                await writer.StoreAsync();
                writer.DetachStream();
                writer.Dispose();
            }
            catch (Exception e)
            {
                if (SocketError.GetStatus(e.HResult) == SocketErrorStatus.Unknown)
                {
                    throw;
                }
            }
        }

        private void SendToAll(List<StreamSocket> destinationSockets, byte[] message)
        {
            foreach(StreamSocket socket in destinationSockets)
            {
                SendTo(socket, message);
            }
        }

        private static byte[] CreateMessageFromPacket(NetworkPacket packet)
        {
            byte[] message = null;
            DataContractSerializer serializer = new DataContractSerializer(packet.GetType(), _knownTypes);

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
                //log.Fatal("Serialization exception", e);
                throw new Exception(e.Message);
            }

            return message;
        }

        private static NetworkPacket ConstructPacketFromMessage(byte[] message)
        {
            NetworkPacket packet = null;
            DataContractSerializer serializer = new DataContractSerializer(typeof(PlayerData), _knownTypes);

            try
            {
                using (MemoryStream stream = new MemoryStream(message))
                {
                    packet = (NetworkPacket)serializer.ReadObject(stream);
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return packet;
        }

        public void Close()
        {
            if (_hostSocket != null)
            {
                _hostSocket.Dispose();
            }
            if (_listener != null)
            {
                _listener.Dispose();
            }
            _listener = null;
            _hostSocket = null;
            Connected = false;
        }

        internal async Task StartListening(string port)
        {

            if (_listener == null && !Connected)
            {
                try
                {
                    _listener = new StreamSocketListener();
                    _listener.ConnectionReceived += OnConnection;
                    await _listener.BindServiceNameAsync(port);
                }
                catch (Exception e)
                {
                    if (SocketError.GetStatus(e.HResult) == SocketErrorStatus.AddressAlreadyInUse)
                    {
                        // Server is already running.
                    }
                    else if (SocketError.GetStatus(e.HResult) == SocketErrorStatus.Unknown)
                    {
                        throw;
                    }

                }
            }
        }

        private async void OnConnection(
            StreamSocketListener sender,
            StreamSocketListenerConnectionReceivedEventArgs args)
        {
            DataReader reader = new DataReader(args.Socket.InputStream);
            try
            {
                while (true)
                {
                    // Read first 4 bytes (length of the subsequent buffer).
                    uint sizeFieldCount = await reader.LoadAsync(sizeof(uint));
                    if (sizeFieldCount != sizeof(uint))
                    {
                        return;
                    }

                    // Read the data
                    uint bufferLength = reader.ReadUInt32();
                    uint actualBufferLength = await reader.LoadAsync(bufferLength);
                    if (bufferLength != actualBufferLength)
                    {
                        return;
                    }
                    byte[] message = new byte[actualBufferLength];
                    reader.ReadBytes(message);

                    NetworkPacket packet = ConstructPacketFromMessage(message);
                    OnPacketReceived(args.Socket, packet);
                }
            }
            catch (Exception exception)
            {
                // If this is an unknown status it means that the error is fatal and retry will likely fail.
                if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
                {
                    throw new Exception(exception.Message);
                }

            }
        }

        public event EventHandler<PacketReceivedEventArgs> PacketReceived = delegate { };
        private void OnPacketReceived(StreamSocket source, NetworkPacket packet)
        {
            PacketReceived?.Invoke(this, new PacketReceivedEventArgs(source, packet));
        }
    }
}
