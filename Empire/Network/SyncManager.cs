using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Empire.Model;
using Empire.Network.PacketTypes;
using System.Threading;
using System.Runtime.CompilerServices;

namespace Empire.Network
{
    internal static class SyncManager
    {
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static TimerCallback callback = SyncTimer;
        private static AutoResetEvent autoEvent = new AutoResetEvent(false);
        private static Timer timer;
        private static UpdateQueue _updateQueue = new UpdateQueue();

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
                _updateQueue.Add(entityPacket);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        internal static UpdateQueue RetrieveUpdatesAndClear()
        {
            UpdateQueue queue = _updateQueue;
            _updateQueue = new UpdateQueue();
            return queue;
        }

        // note that the use of a timer means the callback executes on a different thread
        // so there could be concurrency issues.
        internal static void Start()
        {
            timer = new Timer(SyncTimer, autoEvent, 100, 100);
        }

        internal static void SyncTimer(Object stateInfo)
        {
            Sync();
        }

        internal static void Sync()
        {
            if (ConnectionManager.IsHost)
            {
                foreach (Entity entity in GameModel.GameEntities)
                {
                    EntityPacket box = new EntityPacket(entity);
                    ConnectionManager.SendToAllGamers(box);
                }
            }
        }
        
    }
}
