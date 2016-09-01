using Windows.Networking;
using Windows.Networking.Sockets;
using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.Storage.Streams;

namespace LobbyService
{
    internal class NetworkConnection
    {
        private StreamSocketListener _TCPListener = null;
        private DatagramSocket _UDPListener = null;
        public delegate Task<byte[]> TCPCallback(StreamSocket socket, byte[] data);
        private TCPCallback _tcpCallback;
        public delegate void UDPCallback(DatagramSocket socket, byte[] data);
        private UDPCallback _udpCallback;

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
                if (SocketError.GetStatus(e.HResult) == SocketErrorStatus.Unknown)
                {
                    throw;
                }
            }
            return TCPsocket;
        }

        internal async Task StartTCPListener(string port, TCPCallback handler)
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
                        socket.Dispose();
                        return;
                    }

                    uint dataLength = reader.ReadUInt32();
                    byte[] data = new byte[dataLength];

                    uint actualLength = await reader.LoadAsync(dataLength);
                    if(dataLength != actualLength)
                    {
                        // socket was closed
                        socket.Dispose();
                        return;
                    }
                    reader.ReadBytes(data);

                    // TODO: Most packets sent won't require a response and won't need to be awaited, which blocks the port
                    // consider using a different port with a different handler for those that do.
                    // For those that don't require a response, simply invoke the callback function and move on.
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
                    socket.Dispose();
                }
            }

        }

        internal async Task<byte[]> ConnectAndWaitResponse(string address, string port, byte[] data)
        {
            // create new socket for this request
            using (StreamSocket socket = await ConnectToTCP(address, port))
            {
                await sendTCPData(socket, data);
                byte[] response = await responseFromServer(socket);
                return response;
            }
        }

        private async Task<byte[]> responseFromServer(StreamSocket socket)
        {
            DataReader reader = new DataReader(socket.InputStream);
            byte[] response= new byte[0];

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
            catch (Exception e)
            {
                if (SocketError.GetStatus(e.HResult) == SocketErrorStatus.Unknown)
                {
                    throw;
                }
            }

            return response;
        }

        internal async Task StartUDPListener(string port, UDPCallback handler)
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
                _udpCallback = handler;
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
                _udpCallback(socket, data);
            } catch (Exception e)
            {
                SocketErrorStatus socketError = SocketError.GetStatus(e.HResult);
                throw;
            }

            reader.Dispose();
        }

        internal async Task sendUDPData(string address, string port, byte[] data)
        {
            DatagramSocket socket = new DatagramSocket();
            HostName host = new HostName(address);
            try
            {
                await socket.ConnectAsync(host, port);
            }
            catch {
                throw;
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
                    socket.Dispose();
                }
            }
            socket.Dispose();

        }

        //private async void updateReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        //{
        //    StreamSocket socket = args.Socket;
        //    DataReader reader = new DataReader(socket.InputStream);

        //    try
        //    {
        //        while (true)
        //        {
        //            uint MessageSize = await reader.LoadAsync(sizeof(uint));
        //            if (MessageSize != sizeof(uint))
        //            {
        //                // socket was closed
        //                return;
        //            }

        //            uint dataLength = reader.ReadUInt32();
        //            byte[] data = new byte[dataLength];

        //            MessageSize = await reader.LoadAsync(dataLength);
        //            if (MessageSize != dataLength)
        //            {
        //                return;
        //            }
        //            reader.ReadBytes(data);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception("Exception while reading: ", e);
        //    }
        //}

    }
}