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
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger("NetworkPacket");

        [DataMember]
        internal PacketType Type;
        [DataMember]
        internal IPEndPoint SourceIP;
        [DataMember]
        internal DateTime Timestamp { get; }  // when the packet was sent.  Used for determining lag time and as a packet ID

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
