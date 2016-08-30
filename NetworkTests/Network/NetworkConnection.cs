using Windows.Networking;
using Windows.Networking.Sockets;
using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.Storage.Streams;


//TODO:  instead of specifying an update listener and a request listener, provide AddListener() and RemoveListener() methods whose
// callback signature will determine how to handle the connection.

namespace NetworkTests
{
    internal class NetworkConnection
    {
        private StreamSocketListener _TCPListener = null;
        private DatagramSocket _UDPListener = null;
        public delegate Task<byte[]> Callback(StreamSocket socket, byte[] data);
        private Callback _tcpCallback;

        internal NetworkConnection() {

        }

        internal async Task<StreamSocket> ConnectToTCP(string serverAddress, string serverPort)
        {
            HostName _host = new HostName(serverAddress);
            StreamSocket TCPsocket = new StreamSocket();
            try
            {
                await TCPsocket.ConnectAsync(_host, serverPort);
            }
            catch (Exception e)
            {
                throw new Exception("Error connecting to server: ", e);
            }
            return TCPsocket;
        }

        internal async Task StartTCPListener(string port, Callback handler)
        {
            if (_TCPListener != null)
            {
                _TCPListener.Dispose();
            }

            _TCPListener = new StreamSocketListener();
            _TCPListener.ConnectionReceived += TCPConnectionReceived;

            try
            {
                await _TCPListener.BindServiceNameAsync(port);
            }
            catch (Exception e)
            {
                if (SocketError.GetStatus(e.HResult) == SocketErrorStatus.Unknown)
                {
                    throw;
                }
            }

            _tcpCallback = handler;

        }

        private async void TCPConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
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

                    uint actualLength = await reader.LoadAsync(dataLength);
                    if(dataLength != actualLength)
                    {
                        return;
                    }
                    reader.ReadBytes(data);

                    if (_tcpCallback != null)
                    {
                        byte[] response = await _tcpCallback(socket, data);
                        await sendTCPData(socket, response);
                    }
                }
            }
            catch (Exception e)
            {
                if (SocketError.GetStatus(e.HResult) == SocketErrorStatus.Unknown)
                {
                    throw;
                }
            }
        }

        internal async Task sendTCPData(StreamSocket socket, byte[] data)
        {
            if(socket == null)
            {
                return;
            }

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
                if (SocketError.GetStatus(e.HResult) == SocketErrorStatus.Unknown)
                {
                    throw;
                }
            }

        }

        internal async Task<byte[]> WaitResponse(StreamSocket socket, byte[] data)
        {
            if (socket == null)
            {
                throw new Exception("Attempted to request from a null socket.");
            }
            await sendTCPData(socket, data);
            return await responseFromServer(socket);
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

        internal async Task StartUDPListener(string port)
        {
            if (_UDPListener != null)
            {
                _UDPListener.Dispose();
            }

            _UDPListener = new DatagramSocket();

            try
            {
                _UDPListener.MessageReceived += UDPReceived;
                await _UDPListener.BindServiceNameAsync(port);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to start listening for updates:", e);
            }
        }

        private void UDPReceived(DatagramSocket socket, DatagramSocketMessageReceivedEventArgs args)
        {
            DataReader reader = args.GetDataReader();
            try
            {

                uint len = reader.UnconsumedBufferLength;
                byte[] data = new byte[len];
                reader.ReadBytes(data);
            } catch (Exception e)
            {
                SocketErrorStatus socketError = SocketError.GetStatus(e.HResult);
                throw;
            }

            reader.Dispose();
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
                }
            }
            catch (Exception e)
            {
                throw new Exception("Exception while reading: ", e);
            }
        }

    }
}