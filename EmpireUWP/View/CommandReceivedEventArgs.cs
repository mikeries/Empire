using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EmpireUWP.Network
{
    public class CommandReceivedEventArgs : EventArgs
    {
        public ShipCommand CommandPacket { get; private set; }

        public CommandReceivedEventArgs(ShipCommand packet)
        {
            CommandPacket = packet;
        }
    }
}

