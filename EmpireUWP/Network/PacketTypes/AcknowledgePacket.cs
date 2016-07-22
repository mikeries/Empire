using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EmpireUWP.Network
{
    [DataContract]
    public class AcknowledgePacket : NetworkPacket
    {
        public string Name;

        public AcknowledgePacket() : base()
        {
            Type = PacketType.Acknowledge;
        }

    }
}
