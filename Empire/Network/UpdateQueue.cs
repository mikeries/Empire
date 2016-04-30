using Empire.Network.PacketTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Empire.Network
{
    class UpdateQueue
    {
        private Dictionary<int, EntityPacket> _updateQueue = new Dictionary<int, EntityPacket>();

        internal List<EntityPacket> Packets
        {
            get
            {
                return _updateQueue.Values.ToList();
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        internal void Add(EntityPacket packet)
        {
            int entityID = packet.EnclosedEntity.EntityID;
            if (_updateQueue.ContainsKey(entityID))
            {
                if (packet.IsNewerThan(_updateQueue[entityID]))
                {
                    _updateQueue[entityID] = packet;
                }
                // else ignore the packet as being older than the latest update for this entity
            }
            else
            {
                _updateQueue.Add(entityID, packet);
            }
        }
    }
}
