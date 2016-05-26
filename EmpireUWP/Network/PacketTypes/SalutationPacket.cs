using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EmpireUWP.Network
{
    [DataContract]
    internal class SalutationPacket : NetworkPacket
    {
        [DataMember]
        internal string Name { get; private set; }

        internal SalutationPacket(string name) : base()
        {
            Type = PacketType.Salutation;
            Name = name;
        }

    }
}
