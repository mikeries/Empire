using EmpireUWP.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EmpireUWP.Network
{
    [DataContract]
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

        public bool IsNewerThan(NetworkPacket networkPacket)
        {
            return (this.Timestamp > networkPacket.Timestamp);
        }

    }
}
