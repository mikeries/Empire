using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Empire.Network
{
    public class PacketReceivedEventArgs : EventArgs
    {
        public IPEndPoint Source { get; set; }
        public NetworkPacket Packet { get; set; }

        public PacketReceivedEventArgs(IPEndPoint source, NetworkPacket packet)
        {
            Packet = packet;
            Source = source;
        }
    }
}

