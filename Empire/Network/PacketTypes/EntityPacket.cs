using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Empire.Model;

namespace Empire.Network.PacketTypes
{
    [Serializable]
    class EntityPacket : NetworkPacket
    {
        internal Entity EnclosedEntity { get; private set; }

        internal EntityPacket(Entity entity) : base()
        {
            Type = PacketType.Entity;
            EnclosedEntity = entity;
        }
    }
}
