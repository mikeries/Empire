using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;

namespace LobbyService.Network
{
    internal class PacketReceivedEventArgs : EventArgs
    {
        internal NetworkPacket Packet { get; private set; }

        internal PacketReceivedEventArgs(NetworkPacket packet)
        {
            Packet = packet;
        }
    }
}

