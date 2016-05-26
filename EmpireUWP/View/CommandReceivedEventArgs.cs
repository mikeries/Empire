using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EmpireUWP.Network
{
    internal class CommandReceivedEventArgs : EventArgs
    {
        internal ShipCommand CommandPacket { get; private set; }

        internal CommandReceivedEventArgs(ShipCommand packet)
        {
            CommandPacket = packet;
        }
    }
}

