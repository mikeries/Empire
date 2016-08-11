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
    internal class SyncManager
    {

        private const int MaxSyncDistance = 1000;

        private AutoResetEvent _autoEvent = new AutoResetEvent(false);
        private Timer _timer;
        private UpdateQueue _updateQueue = new UpdateQueue();
        private int _lastUpdated = 0;
        private ConnectionManager _connectionManager;
        private GameModel _gameModel;

        public SyncManager(GameModel gameModel)
        {
            _gameModel = gameModel;
            _gameModel.EntitiesRemoved += RemoveEntities;
        }

        // TODO: figure out how to do this with async/await tasks
        internal void Start(ConnectionManager connection)
        {
            if (connection != null)
            {
                _connectionManager = connection;
                //_connectionManager.GetNetworkConnection.PacketReceived += ProcessIncomingPacket;
                _timer = new Timer(SyncTimer, _autoEvent, 50, 50);
            }
        }

        internal async void SyncTimer(object stateInfo)
        {
            if (_connectionManager.Hosting)
            {
                PlayerData player = NextGamerToUpdate();

                if (player != null)
                {
                    await SyncPlayer(player.PlayerID);
                }
            }
        }

        private void ProcessIncomingPacket(object sender, PacketReceivedEventArgs e)
        {
            NetworkPacket packet = e.Packet;

            if (packet.Type == PacketType.Entity)
            {
                EntityPacket entityPacket = packet as EntityPacket;
                _updateQueue.Add(entityPacket);
            }
        }

        internal UpdateQueue RetrieveUpdatesAndClear()
        {
            UpdateQueue queue = _updateQueue;
            _updateQueue = new UpdateQueue();
            return queue;
        }

        private PlayerData NextGamerToUpdate()
        {
            List<PlayerData> gamers = _connectionManager.Gamers;

            if (gamers.Count <= 1)
            {
                return null;
            }

            IncrementGamerIndex(gamers);
            if (gamers[_lastUpdated].PlayerID == _connectionManager.LocalPlayerID)
            {
                IncrementGamerIndex(gamers);
            }
            return gamers[_lastUpdated];
        }

        private void IncrementGamerIndex(List<PlayerData> gamers)
        {
            _lastUpdated++;
            if (_lastUpdated >= gamers.Count)
            {
                _lastUpdated = 0;
            }
        }

        internal async Task SyncAll()
        {
            if (_connectionManager.Hosting)
            {
                foreach (Entity entity in _gameModel.GameEntities)
                {
                    EntityPacket packet = new EntityPacket(entity);
                    await _connectionManager.SendPacketToAllClients(packet);
                }
            }
        }
        
        private async Task SyncPlayer(string player)
        {
            Ship ship = _connectionManager.GetShip(player);
            List<EntityPacket> updates = new List<EntityPacket>();
            foreach(Entity entity in _gameModel.GameEntities)
            {
                if (DistanceBetween(ship, entity) < MaxSyncDistance)
                {
                    EntityPacket packet = new EntityPacket(entity);
                    updates.Add(packet);
                }
            }
           await _connectionManager.SendSyncUpdatesToPlayer(player, updates);
        }

        internal async Task RemoveEntity(Entity entity)
        {
            if (_connectionManager.Hosting)
            {
                EntityPacket packet = new EntityPacket(entity);
                await _connectionManager.SendPacketToAllClients(packet);
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
        private async void RemoveEntities(object sender, EntitiesRemovedEventAgs e)
        {
            List<Entity> entitiesToRemove = e.EntitiesRemoved;
            for (int i = 0; i < 5; i++)
            {
                foreach (Entity entity in entitiesToRemove)
                {
                    EntityPacket packet = new EntityPacket(entity);
                    await _connectionManager.SendPacketToAllClients(packet);
                }
            }
        }
    }
}
