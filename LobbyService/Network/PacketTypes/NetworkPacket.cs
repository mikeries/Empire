using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LobbyService
{
    [DataContract (Namespace="http://schemas.datacontract.org/2004/07/EmpireSerializer")]
    public abstract class NetworkPacket
    {

        [DataMember]
        public PacketType Type;
        [DataMember]
        public DateTime Timestamp { get; private set; }  // when the packet was sent.  Used for determining lag time and as a packet ID

        public NetworkPacket()
        {
            Timestamp = DateTime.Now;
        }

    }
}
