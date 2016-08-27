using Windows.Networking;
using Windows.Networking.Sockets;
using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.Storage.Streams;


//TODO:  instead of specifying an update listener and a request listener, provide AddListener() and RemoveListener() methods whose
// callback signature will determine how to handle the connection.  Four possibilities come to mind:
// delegate Task<byte[]> DataRequestCallback(byte[] data) for requests wanting to be called with raw data
// delegate Task<NetworkPacket> PacketRequestCallback(NetworkPacket packet) for requests wanting to deal only with packets
// delegate void DataUpdateCallback for updates providing raw data
// delegate void PacketUpdateCallback for updates to packets.

namespace EmpireUWP.Network
{
    internal class NetworkConnection
    {
        private StreamSocketListener _updateListener;
        private StreamSocketListener _requestListener;
        internal delegate Task<byte[]> RequestCallback(byte[] data);
        RequestCallback _requestCallback;
        internal delegate void UpdateCallback(byte[] data);
        UpdateCallback _updateCallback;
        internal delegate Task<NetworkPacket> PacketRequestCallback(NetworkPacket packet);
        PacketRequestCallback _packetRequestCallback;
        internal delegate void PacketUpdateCallback(NetworkPacket packet);
        PacketUpdateCallback _packetUpdateCallback;
        private ISerializer _serializer;

        internal NetworkConnection(ISerializer serializer) {
            if (serializer == null)
            {
                throw new ArgumentNullException("Invalid Serializer (Null).");
            }
                
            _serializer = serializer;
        }

        internal async Task StartRequestListener(string port, RequestCallback callBack)
        {
            if (_requestListener != null)
            {
                _requestListener.Dispose();
            }
            _requestListener = await createListener(port);
            _requestListener.ConnectionReceived += requestReceived;

            _requestCallback = callBack;
        }

        internal async Task StartRequestListener(string port, PacketRequestCallback callBack)
        {
            if (_requestListener != null)
            {
                _requestListener.Dispose();
            }
            _requestListener = await createListener(port);
            _requestListener.ConnectionReceived += requestReceived;

            _packetRequestCallback = callBack;
        }

        internal async Task StartUpdateListener(string port, UpdateCallback updateCallback)
        {
            if (_updateListener != null)
            {
                _updateListener.Dispose();
            }

            _updateListener = await createListener(port);
            _updateListener.ConnectionReceived += updateReceived;

            _updateCallback = updateCallback;
        }

        internal async Task StartUpdateListener(string port, PacketUpdateCallback updateCallback)
        {
            if (_updateListener != null)
            {
                _updateListener.Dispose();
            }

            _updateListener = await createListener(port);
            _updateListener.ConnectionReceived += updateReceived;

            _packetUpdateCallback = updateCallback;
        }

        private async Task<StreamSocketListener> createListener(string port)
        {
            StreamSocketListener listener = new StreamSocketListener();
            try
            {
                await listener.BindServiceNameAsync(port);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to start listening for requests:", e);
            }
            return listener;
        }

        private async void requestReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            StreamSocket socket = args.Socket;
            DataReader reader = new DataReader(socket.InputStream);

            try
            {
                while (true)
                {
                    uint MessageSize = await reader.LoadAsync(sizeof(uint));
                    if (MessageSize != sizeof(uint))
                    {
                        // socket was closed
                        return;
                    }

                    uint dataLength = reader.ReadUInt32();
                    byte[] data = new byte[dataLength];

                    await reader.LoadAsync(dataLength);
                    reader.ReadBytes(data);

                    if (_requestCallback != null)
                    {
                        byte[] response = await _requestCallback(data);
                        await sendResponse(socket, response);
                    }
                    else if (_packetRequestCallback != null)
                    {
                        NetworkPacket packet = _serializer.ConstructPacketFromMessage(data);
                        NetworkPacket responsePacket = await _packetRequestCallback(packet);

                        byte[] responseData = _serializer.CreateMessageFromPacket(responsePacket);
                        await sendResponse(socket, responseData);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Exception while reading: " + e.Message, e);
            }
        }

        private async Task sendResponse(StreamSocket socket, byte[] data)
        {
            byte[] dataToSend = data;

            DataWriter writer = new DataWriter(socket.OutputStream);

            writer.WriteUInt32((uint)dataToSend.Length);
            writer.WriteBytes(dataToSend);

            try
            {
                await writer.StoreAsync();
                await writer.FlushAsync();
                writer.DetachStream();
            }
            catch (Exception e)
            {
                throw new Exception("Send failed with Message: ", e);
            }

        }

        internal Task SendUpdate(StreamSocket socket, byte[] data)
        {
            return sendResponse(socket, data);
        }

        internal Task SendUpdatePacket(StreamSocket socket, NetworkPacket packet)
        {
            byte[] data = _serializer.CreateMessageFromPacket(packet);
            return sendResponse(socket, data);
        }

        private async void updateReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            StreamSocket socket = args.Socket;
            DataReader reader = new DataReader(socket.InputStream);

            try
            {
                while (true)
                {
                    uint MessageSize = await reader.LoadAsync(sizeof(uint));
                    if (MessageSize != sizeof(uint))
                    {
                        // socket was closed
                        return;
                    }

                    uint dataLength = reader.ReadUInt32();
                    byte[] data = new byte[dataLength];

                    MessageSize = await reader.LoadAsync(dataLength);
                    if (MessageSize != dataLength)
                    {
                        return;
                    }
                    reader.ReadBytes(data);

                    if (_updateCallback != null)
                    {
                        _updateCallback(data);
                    }
                    else if (_packetUpdateCallback != null)
                    {
                        NetworkPacket packet = _serializer.ConstructPacketFromMessage(data);
                        _packetUpdateCallback(packet);
                        MessageSize = 5;
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Exception while reading: ", e);
            }
        }

        internal async Task<byte[]> WaitResponse(StreamSocket socket, byte[] data)
        {
            if (socket == null)
            {
                throw new Exception("Attempted to request from a null socket.");
            }
            await sendRequest(socket, data);
            return await responseFromServer(socket);
        }

        internal async Task<NetworkPacket> WaitResponsePacket(StreamSocket socket, NetworkPacket packet)
        {
            byte[] message = _serializer.CreateMessageFromPacket(packet);
            byte[] response = await WaitResponse(socket, message);

            NetworkPacket responsePacket = _serializer.ConstructPacketFromMessage(response);
            return responsePacket;
        }

        internal async Task<StreamSocket> Connect(string serverAddress, string serverPort)
        {
            HostName _host = new HostName(serverAddress);
            StreamSocket socket = new StreamSocket();
            try
            {
                await socket.ConnectAsync(_host, serverPort);
            }
            catch (Exception e)
            {
                throw new Exception("Error connecting to server: ", e);
            }
            return socket;
        }

        private async Task sendRequest(StreamSocket socket, byte[] data)
        {
            DataWriter writer = new DataWriter(socket.OutputStream);

            writer.WriteUInt32((uint)data.Length);
            writer.WriteBytes(data);

            try
            {
                await writer.StoreAsync();
                writer.DetachStream();
            }
            catch (Exception e)
            {
                throw new Exception("Failed to send request message.", e);
            }
        }

        private async Task<byte[]> responseFromServer(StreamSocket socket)
        {
            DataReader reader = new DataReader(socket.InputStream);
            byte[] response;

            try
            {
                uint size = await reader.LoadAsync(sizeof(uint));
                if (size != sizeof(int))
                {
                    throw new Exception("Socket closed unexpectedly.");
                }

                uint dataLength = reader.ReadUInt32();
                response = new byte[dataLength];
                await reader.LoadAsync(dataLength);

                reader.ReadBytes(response);
            }
            catch
            {
                throw;
            }

            return response;
        }

    }
}