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
    abstract class NetworkPacket
    {

        [DataMember]
        internal PacketType Type;
        [DataMember]
        internal DateTime Timestamp { get; private set; }  // when the packet was sent.  Used for determining lag time and as a packet ID

        internal NetworkPacket()
        {
            Timestamp = DateTime.Now;
        }

        internal virtual bool IsValid()
        {
            return true;
        }

        internal bool IsNewerThan(NetworkPacket networkPacket)
        {
            return (this.Timestamp > networkPacket.Timestamp);
        }

    }
}
