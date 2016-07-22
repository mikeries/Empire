using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EmpireUWP.Network
{
    [DataContract]
    internal class PingPacket : NetworkPacket
    {
        internal PingPacket() : base()
        {
            this.Type = PacketType.Ping;
        }

    }
}
