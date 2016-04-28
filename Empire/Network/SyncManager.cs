using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Empire.Model;
using Empire.Network.PacketTypes;
using System.Threading;

namespace Empire.Network
{
    internal static class SyncManager
    {
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static TimerCallback callback = SyncShips;
        private static AutoResetEvent autoEvent = new AutoResetEvent(false);
        private static Timer timer;

        static SyncManager()
        {
            NetworkInterface.PacketReceived += ProcessIncomingPacket;
        }

        private static void ProcessIncomingPacket(object sender, PacketReceivedEventArgs e)
        {
            NetworkPacket packet = e.Packet;
            packet.Source = e.Source;

            if (packet.Type == PacketType.Entity)
            {
                EntityPacket entityPacket = packet as EntityPacket;
                Entity entity = entityPacket.EnclosedEntity;
                GameModel.UpdateEntity(entity);
            }
        }

        // note that the use of a timer means the callback executes on a different thread
        // so there could be concurrency issues.
        internal static void Start()
        {
            timer = new Timer(SyncShips, autoEvent, 1000, 1000);
        }

        internal static void SyncShips(Object stateInfo)
        {
            if (ConnectionManager.IsHost)
            {
                //int entityCount = 0;
                //foreach (Entity entity in GameModel.GameEntities)
                //{
                //    EntityPacket box = new EntityPacket(entity);
                //    ConnectionManager.SendToAllGamers(box);
                //    entityCount++;
                //}

                foreach(Entity entity in GameModel.Ships)
                {
                    EntityPacket box = new EntityPacket(entity);
                    ConnectionManager.SendToAllGamers(box);
                }
            }
        }
        
    }
}
