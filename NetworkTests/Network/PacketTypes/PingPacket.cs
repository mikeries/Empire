using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTests
{
    [DataContract]
    public class PingPacket : NetworkPacket
    {
        public PingPacket() : base()
        {
            this.Type = PacketType.Ping;
        }

    }
}
