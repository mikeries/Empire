using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace LobbyClient
{
    public class LobbyService
    {

        MainPage rootPage = null;
        StreamSocket socket = null;
        DataWriter writer = null;
        StreamSocketListener listener = null;

        public LobbyService()
        {
            rootPage = MainPage.Current;
            socket = null;
            writer = null;
            listener = null;
        }

        public async void StartListening(string port)
        {
            if (listener == null)
            {
                try
                {
                    listener = new StreamSocketListener();
                    listener.ConnectionReceived += OnConnection;
                    listener.Control.KeepAlive = false;
                    await listener.BindServiceNameAsync(port);

                    rootPage.NotifyUser("Listening...");
                }
                catch (Exception e)
                {
                    if (SocketError.GetStatus(e.HResult ) == SocketErrorStatus.Unknown)
                    {
                        throw;
                    }

                    rootPage.NotifyUser("StartListening failed with an error: " + e.Message);
                }
            }

        }

        public async void Connect(string hostAddress, string port)
        {
            if(socket != null)
            {
                rootPage.NotifyUser("Already connected.");
                return;
            }

            try
            {
                socket = new StreamSocket();
                socket.Control.KeepAlive = false;
                HostName hostName = new HostName(hostAddress);
                rootPage.NotifyUser("Connecting...");
                await socket.ConnectAsync(hostName, port);
                writer = new DataWriter(socket.OutputStream);
                rootPage.NotifyUser("Connected");
            }
            catch (Exception e)
            {
                if(SocketError.GetStatus(e.HResult) == SocketErrorStatus.Unknown)
                {
                    throw;
                }
                rootPage.NotifyUser("Connect failed with error: " + e.Message);
            }

        }

        public async void Send(string message)
        {
            if (writer == null)
            {
                rootPage.NotifyUser("Please connect first.");
                return;
            }

            writer.WriteUInt32(writer.MeasureString(message));
            writer.WriteString(message);

            try
            {
                await writer.StoreAsync();
                rootPage.NotifyUser("Sent '" + message + "' successfully.");
            }
            catch (Exception e)
            {
                if (SocketError.GetStatus(e.HResult) == SocketErrorStatus.Unknown)
                {
                    throw;
                }
                rootPage.NotifyUser("Send failed with error: " + e.Message);
            }
        }

        public void Close()
        {
            if (writer!=null)
            {
                writer.DetachStream();
                writer.Dispose();
                rootPage.NotifyUser("DataWriter closed.");
            }
            if (socket!=null)
            {
                socket.Dispose();
                rootPage.NotifyUser("Socket closed.");
            }
            if (listener != null)
            {
                listener.Dispose();
                rootPage.NotifyUser("No longer listening.");
            }
            writer = null;
            listener = null;
            socket = null;
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
                    // Read first 4 bytes (length of the subsequent string).
                    uint sizeFieldCount = await reader.LoadAsync(sizeof(uint));
                    if (sizeFieldCount != sizeof(uint))
                    {
                        return;
                    }

                    // Read the string.
                    uint stringLength = reader.ReadUInt32();
                    uint actualStringLength = await reader.LoadAsync(stringLength);
                    if (stringLength != actualStringLength)
                    {
                        return;
                    }

                    rootPage.NotifyUserFromAsyncThread("Received message: " + reader.ReadString(actualStringLength));

                }
            }
            catch (Exception exception)
            {
                // If this is an unknown status it means that the error is fatal and retry will likely fail.
                if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
                {
                    throw;
                }

            }
        }

    }
}
