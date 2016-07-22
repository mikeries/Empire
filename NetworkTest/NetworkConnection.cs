using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
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
            typeof(GameData),
            typeof(LobbyData),
            typeof(LobbyCommandPacket),
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
                await _hostSocket.ConnectAsync(hostName, port).AsTask();
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

        public Task SendPacketToHost(NetworkPacket packet)
        {
            return SendPacketTo(_hostSocket, packet);
        }

        public Task SendPacketTo(StreamSocket destinationSocket, NetworkPacket packet)
        {
            byte[] message = CreateMessageFromPacket(packet);
            return SendTo(destinationSocket, message);
        }

        public Task SendPacketToAll(List<StreamSocket> destinationSockets, NetworkPacket packet)
        {
            byte[] message = CreateMessageFromPacket(packet);
            return SendToAll(destinationSockets, message);
        }

        public async Task WaitResponse(NetworkPacket packet, StreamSocket destinationSocket = null)
        {
            if (destinationSocket == null)
            {
                destinationSocket = _hostSocket;
            }

            byte[] message = CreateMessageFromPacket(packet);
            await SendTo(destinationSocket, message);

            byte[] response = ReadBuffer(destinationSocket).Result;

        }

        private async Task<byte[]> ReadBuffer(StreamSocket socket)
        {
            DataReader reader = new DataReader(socket.InputStream);

            try
            {
                // Read length of the subsequent buffer.
                await reader.LoadAsync(sizeof(uint));
                uint bufferLength = reader.ReadUInt32();

                uint actualBufferLength = 0;
                while (bufferLength != actualBufferLength)
                {
                        actualBufferLength = await reader.LoadAsync(bufferLength);
                }
                byte[] message = new byte[actualBufferLength];
                reader.ReadBytes(message);

                return message;
            }
            catch
            {
                throw;
            } finally
            {
                reader.Dispose();
            }

        }

        private async Task SendTo(StreamSocket destinationSocket, byte[] message)
        {
            if (destinationSocket==null || !Connected)
            {
                return;
            }

            DataWriter writer = new DataWriter(destinationSocket.OutputStream);

            writer.WriteUInt32((UInt32)message.Length);
            writer.WriteBytes(message);

            try
            {
                await writer.StoreAsync();
            }
            catch (Exception e)
            {
                if (SocketError.GetStatus(e.HResult) == SocketErrorStatus.Unknown)
                {
                    throw;
                }
            }
            finally
            {
                writer.DetachStream();
                writer.Dispose();
            }
        }

        private async Task SendToAll(List<StreamSocket> destinationSockets, byte[] message)
        {
            foreach(StreamSocket socket in destinationSockets)
            {
                await SendTo(socket, message);
            }
        }

        private static byte[] CreateMessageFromPacket(NetworkPacket packet)
        {
            byte[] message = null;
            DataContractSerializer serializer = new DataContractSerializer(typeof(NetworkPacket), _knownTypes);

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
            DataContractSerializer serializer = new DataContractSerializer(typeof(NetworkPacket), _knownTypes);

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

        internal async Task StartListeningAsync(string port)
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
                        _listener.Dispose();
                        throw;
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
                    // Read length of the subsequent buffer.
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
