using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire.Network
{
    [Serializable]
    public class PingPacket : NetworkPacket
    {
        public PingPacket() : base()
        {
            Type = PacketType.Ping;
        }

    }
}
