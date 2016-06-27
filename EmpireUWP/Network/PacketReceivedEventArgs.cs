using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;

namespace EmpireUWP.Network
{
    internal class PacketReceivedEventArgs : EventArgs
    {
        internal StreamSocket Source { get; private set; }
        internal NetworkPacket Packet { get; private set; }

        internal PacketReceivedEventArgs(StreamSocket source, NetworkPacket packet)
        {
            Packet = packet;
            Source = source;
        }
    }
}

