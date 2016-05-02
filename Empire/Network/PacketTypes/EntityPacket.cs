using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Empire.Model;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Empire.Network.PacketTypes
{
    [Serializable]
    class EntityPacket : NetworkPacket
    {
        // Implementation note
        // Rather than enclose an Entity object, we instead serialize the entity and pass the message which can be used to
        // initialize an existing object from an object pool rather than contstructing a new one.  This avoids
        // excessive heap fragmentation.
        internal ObjectState EntityState;
        internal EntityType EntityType;
        internal int EntityID;

        internal EntityPacket(Entity entity) : base()
        {
            Type = PacketType.Entity;
            EntityType = entity.Type;
            EntityID = entity.EntityID;

            ObjectState entityState = new ObjectState();
            entity.GetState(entityState);
            EntityState = entityState;
        }

    }
}
