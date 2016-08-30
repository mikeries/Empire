using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LobbyTest
{
    [DataContract]
    public class AcknowledgePacket : NetworkPacket
    {
        [DataMember]
        public string Name;

        public AcknowledgePacket() : base()
        {
            Type = PacketType.Acknowledge;
        }

    }
}
