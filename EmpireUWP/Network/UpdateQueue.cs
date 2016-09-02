using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EmpireUWP.Network
{
    class UpdateQueue
    {
        private ConcurrentDictionary<int, EntityPacket> _updateQueue = new ConcurrentDictionary<int, EntityPacket>();
        internal List<int> _deadEntities = new List<int>();

        internal List<EntityPacket> Packets
        {
            get
            {
                return _updateQueue.Values.ToList();
            }
        }

        internal void Add(EntityPacket packet)
        {
            int entityID = packet.EntityID;
            if (_updateQueue.ContainsKey(entityID))
            {
                _updateQueue[entityID] = packet;
            }
            else
            {
                _updateQueue.TryAdd(entityID, packet);
            }
        }
    }
}
