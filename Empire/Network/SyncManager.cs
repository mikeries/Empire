using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Empire.Model;
using Empire.Network;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Net;
using System.Collections;
using Empire.Network.PacketTypes;
using System.Collections.Concurrent;

namespace Empire.Network
{
    internal static class SyncManager
    {
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const int MaxSyncDistance = 1000;

        private static TimerCallback _callback = SyncTimer;
        private static AutoResetEvent _autoEvent = new AutoResetEvent(false);
        private static Timer _timer;
        private static UpdateQueue _updateQueue = new UpdateQueue();
        private static int _lastUpdated = 0;

        static SyncManager()
        {
            NetworkInterface.PacketReceived += ProcessIncomingPacket;
        }

        private static void ProcessIncomingPacket(object sender, PacketReceivedEventArgs e)
        {
            NetworkPacket packet = e.Packet;
            packet.SourceIP = e.SourceIP;

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


        // TODO: figure out how to do this with a thread that just contains a while and updates players as fast as it can
        // then figure out how to get it to go as fast as it can, but with a 'minumum' time between updates so that it doesn't
        // eat all available bandwidth/CPU time.
        // Need to remember to have a way to stop it.
        internal static void Start()
        {
            _timer = new Timer(SyncTimer, _autoEvent, 50, 50);
        }

        internal static void SyncTimer(object stateInfo)
        {
            if (ConnectionManager.IsHost)
            {
                ConnectionManager.SendPlayerDataToAll();

                Gamer player = NextGamerToUpdate();

                if (player != null)
                {
                    SyncPlayer(player.ConnectionID);
                }
            }
        }

        private static Gamer NextGamerToUpdate()
        {
            List<Gamer> gamers = ConnectionManager.Gamers.Values.ToList();

            if (gamers.Count <= 1)
            {
                return null;
            }

            IncrementGamerIndex(gamers);
            if (gamers[_lastUpdated].ConnectionID == ConnectionManager.ConnectionID)
            {
                IncrementGamerIndex(gamers);
            }
            return gamers[_lastUpdated];
        }

        private static void IncrementGamerIndex(List<Gamer> gamers)
        {
            _lastUpdated++;
            if (_lastUpdated >= gamers.Count)
            {
                _lastUpdated = 0;
            }
        }

        internal static void SyncAll()
        {
            if (ConnectionManager.IsHost)
            {
                foreach (Entity entity in GameModel.GameEntities)
                {
                    EntityPacket packet = new EntityPacket(entity);
                    ConnectionManager.SendToAllGamers(packet);
                }
            }
        }
        
        private static void SyncPlayer(string player)
        {
            Ship ship = ConnectionManager.GetShip(player);
            List<EntityPacket> updates = new List<EntityPacket>();
            foreach(Entity entity in GameModel.GameEntities)
            {
                if (DistanceBetween(ship, entity) < MaxSyncDistance)
                {
                    EntityPacket packet = new EntityPacket(entity);
                    updates.Add(packet);
                }
            }
            ConnectionManager.SendSyncUpdatesToPlayer(player, updates);
        }

        internal static void RemoveEntities(List<Entity> deadEntities)
        {
            throw new NotImplementedException();
        }

        internal static void RemoveEntity(Entity entity)
        {
            if (ConnectionManager.IsHost)
            {
                EntityPacket packet = new EntityPacket(entity);
                ConnectionManager.SendToAllGamers(packet);
            }
        }

        private static int DistanceBetween(Ship ship, Entity entity)
        {
            return (int)(ship.Location - entity.Location).Length();
        }
    }
}
