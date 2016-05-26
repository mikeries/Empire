using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EmpireUWP.Network
{
    internal class PacketReceivedEventArgs : EventArgs
    {
        internal IPEndPoint SourceIP { get; private set; }
        internal NetworkPacket Packet { get; private set; }

        internal PacketReceivedEventArgs(IPEndPoint source, NetworkPacket packet)
        {
            Packet = packet;
            SourceIP = source;
        }
    }
}

