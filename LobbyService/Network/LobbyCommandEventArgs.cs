using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;

namespace LobbyService
{
    internal class LobbyCommandEventArgs : EventArgs
    {
        internal LobbyCommandPacket Packet { get; private set; }

        internal LobbyCommandEventArgs(LobbyCommandPacket packet)
        {
            Packet = packet;
        }
    }
}