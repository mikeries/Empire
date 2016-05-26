using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmpireUWP.Network
{
    [Serializable]
    internal class SalutationPacket : NetworkPacket
    {
        internal string Name { get; private set; }

        internal SalutationPacket(string name) : base()
        {
            Type = PacketType.Salutation;
            Name = name;
        }

    }
}
