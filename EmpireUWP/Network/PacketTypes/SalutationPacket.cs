using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EmpireUWP.Network
{
    [DataContract (Namespace="http://schemas.datacontract.org/2004/07/EmpireSerializer")]
    public class SalutationPacket : NetworkPacket
    {
        [DataMember]
        public string Name { get; private set; }

        public SalutationPacket(string name) : base()
        {
            Type = PacketType.Salutation;
            Name = name;
        }

    }
}
