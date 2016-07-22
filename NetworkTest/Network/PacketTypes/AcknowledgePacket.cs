using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EmpireUWP.Network
{
    [DataContract]
    class AcknowledgePacket : NetworkPacket
    {

        public AcknowledgePacket() : base()
        {
            Type = PacketType.Acknowledge;
        }

    }
}
