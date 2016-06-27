using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmpireUWP.Model;
using EmpireUWP.Network;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Net;
using System.Collections;
using System.Collections.Concurrent;

namespace EmpireUWP.Network
{
    internal static class SyncManager
    {

        private const int MaxSyncDistance = 1000;

        private static TimerCallback _callback = SyncTimer;
        private static AutoResetEvent _autoEvent = new AutoResetEvent(false);
        private static Timer _timer;
        private static UpdateQueue _updateQueue = new UpdateQueue();
        private static int _lastUpdated = 0;
        private static ConnectionManager _connectionManager;

        static SyncManager()
        {
            GameModel.EntitiesRemoved += RemoveEntities;
        }

        private static void ProcessIncomingPacket(object sender, PacketReceivedEventArgs e)
        {
            NetworkPacket packet = e.Packet;
            //packet.SourceIP = e.SourceIP;

            if (packet.Type == PacketType.Entity)
            {
                EntityPacket entityPacket = packet as EntityPacket;
                _updateQueue.Add(entityPacket);
            }
        }

        internal static UpdateQueue RetrieveUpdatesAndClear()
        {
            UpdateQueue queue = _updateQueue;
            _updateQueue = new UpdateQueue();
            return queue;
        }

        // TODO: figure out how to do this with async/await tasks
        internal static void Start(ConnectionManager connection)
        {
            _connectionManager = connection;
            _connectionManager.GetNetworkConnection.PacketReceived += ProcessIncomingPacket;
            _timer = new Timer(SyncTimer, _autoEvent, 50, 50);
        }

        internal static void SyncTimer(object stateInfo)
        {
            if (_connectionManager.Host)
            {
                Player player = NextGamerToUpdate();

                if (player != null)
                {
                    SyncPlayer(player.PlayerID);
                }
            }
        }

        private static Player NextGamerToUpdate()
        {
            List<Player> gamers = _connectionManager.Gamers;

            if (gamers.Count <= 1)
            {
                return null;
            }

            IncrementGamerIndex(gamers);
            if (gamers[_lastUpdated].PlayerID == _connectionManager.PlayerID)
            {
                IncrementGamerIndex(gamers);
            }
            return gamers[_lastUpdated];
        }

        private static void IncrementGamerIndex(List<Player> gamers)
        {
            _lastUpdated++;
            if (_lastUpdated >= gamers.Count)
            {
                _lastUpdated = 0;
            }
        }

        internal static void SyncAll()
        {
            if (_connectionManager.Host)
            {
                foreach (Entity entity in GameModel.GameEntities)
                {
                    EntityPacket packet = new EntityPacket(entity);
                    _connectionManager.SendPacketToAllClients(packet);
                }
            }
        }
        
        private static void SyncPlayer(string player)
        {
            Ship ship = _connectionManager.GetShip(player);
            List<EntityPacket> updates = new List<EntityPacket>();
            foreach(Entity entity in GameModel.GameEntities)
            {
                if (DistanceBetween(ship, entity) < MaxSyncDistance)
                {
                    EntityPacket packet = new EntityPacket(entity);
                    updates.Add(packet);
                }
            }
            _connectionManager.SendSyncUpdatesToPlayer(player, updates);
        }

        internal static void RemoveEntity(Entity entity)
        {
            if (_connectionManager.Host)
            {
                EntityPacket packet = new EntityPacket(entity);
                _connectionManager.SendPacketToAllClients(packet);
            }
        }

        private static int DistanceBetween(Ship ship, Entity entity)
        {
            return (int)(ship.Location - entity.Location).Length();
        }

        // Note:  Technically, all the clients need is a list of entities IDs
        // and they could be passed within a single packet which would save bandwidth.
        // But this works too and is simpler.

        // TODO: Develop TCP networking capability, as UDP is not reliable enough in
        // this case.  (For now, let's just send the packets 5X)
        private static void RemoveEntities(object sender, EntitiesRemovedEventAgs e)
        {
            List<Entity> entitiesToRemove = e.EntitiesRemoved;
            for (int i = 0; i < 5; i++)
            {
                foreach (Entity entity in entitiesToRemove)
                {
                    EntityPacket packet = new EntityPacket(entity);
                    _connectionManager.SendPacketToAllClients(packet);
                }
            }
        }
    }
}
