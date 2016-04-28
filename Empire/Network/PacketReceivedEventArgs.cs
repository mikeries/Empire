using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Empire.Network
{
    internal class PacketReceivedEventArgs : EventArgs
    {
        internal IPEndPoint Source { get; set; }
        internal NetworkPacket Packet { get; set; }

        internal PacketReceivedEventArgs(IPEndPoint source, NetworkPacket packet)
        {
            Packet = packet;
            Source = source;
        }
    }
}

