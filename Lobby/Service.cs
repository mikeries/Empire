using System;
using Windows.Networking.Sockets;

namespace Lobby
{
    public class LobbyService
    {
        public void Start()
        {
            StreamSocketListener listener = new StreamSocketListener();
            listener.ConnectionReceived += OnConnection;

            Console.WriteLine("Listening...");

        }
    }
}

