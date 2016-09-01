﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EmpireUWP.Model;
using EmpireUWP.View;
using System.Collections.Concurrent;
using Windows.Networking.Sockets;
using Windows.Networking;
using System.Threading;

namespace EmpireUWP.Network
{
    public class GameServer
    {
        private const int MaxSyncDistance = 1000;

        internal List<PlayerData> Gamers { get { return _playerList.Values.ToList(); } }

        private Dictionary<string, PlayerData> _playerList;
        private GameData _gameData;
        private GameView _gameInstance;
        private Timer _timer;
        private AutoResetEvent _autoEvent = new AutoResetEvent(false);

        private PacketConnection _networkConnection;

        internal GameServer(GameView gameInstance, Dictionary<string, PlayerData> players, GameData gameData)
        {
            _gameInstance = gameInstance;
            _playerList = players;
            _gameData = gameData;
        }

        internal async Task StartServer()
        {
            EmpireSerializer serializer = new Network.EmpireSerializer();
            _networkConnection = new PacketConnection(serializer);

            await _networkConnection.StartTCPListener(_gameData.HostPort, HandleRequest);

            _timer = new Timer(SyncTimer, _autoEvent, 1000, 1000);
        }

        internal async void SyncTimer(object stateInfo)
        {
            if (_gameInstance.Hosting)
            {
                foreach (PlayerData player in _playerList.Values)
                {
                    if (player != null && _gameData.HostID != player.PlayerID)
                    {
                        await SyncPlayer(player.PlayerID);
                    }

                    GameServerDataUpdate update = new GameServerDataUpdate(_playerList, _gameData);
                    await SendPacketToPlayer(player, update);
                }
            }
        }

        private Task SendPacketToPlayer(PlayerData player, NetworkPacket packet)
        {
            return _networkConnection.SendTCPData(player.ClientSocket, packet);
        }

        private async Task SendPacketToAllPlayers(NetworkPacket packet)
        {
            foreach(PlayerData player in _playerList.Values)
            {
                await SendPacketToPlayer(player, packet);
            }
        }

        internal Task SyncPlayer(string player)
        {
            Ship ship = _gameInstance.GameClientConnection.GetShip(player);
            List<NetworkPacket> updates = new List<NetworkPacket>();
            PlayerData playerToSync = _playerList[player];

            foreach (Entity entity in _gameInstance.GameModel.GameEntities)
            {
                if (DistanceBetween(ship, entity) < 100000000)
                {
                    EntityPacket packet = new EntityPacket(entity);
                    updates.Add(packet);
                }
            }
            return SendUpdatesToPlayer(playerToSync, updates);
        }

        internal async Task SendUpdatesToPlayer(PlayerData player, List<NetworkPacket> updates)
        {
            PlayerData playerToUpdate = player;

            if (player.Connected)
            {
                foreach (NetworkPacket update in updates)
                {
                    await SendPacketToPlayer(player, update);
                }
            }
        }

        private async Task<NetworkPacket> HandleRequest(StreamSocket socket, NetworkPacket packet)
        {
            AcknowledgePacket acknowledgement = new AcknowledgePacket();

            if (packet.Type == PacketType.Salutation)
            {
                SalutationPacket salutation = packet as SalutationPacket;
                await HandlePlayerConnection(socket, salutation);
            } else if (packet.Type == PacketType.PlayerData)
            {
                PlayerData playerData = packet as PlayerData;
                if (_playerList.ContainsKey(playerData.PlayerID))
                {
                    _playerList[playerData.PlayerID] = playerData;
                }
            }
            else if (packet.Type == PacketType.ShipCommand)
            {
                await SendPacketToAllPlayers(packet);
            }

            return acknowledgement;
        }

        private async Task HandlePlayerConnection(StreamSocket socket, SalutationPacket salutation)
        {
            string PlayerID = salutation.Name;
            if(_playerList.ContainsKey(PlayerID))
            {
                PlayerData playerData = _playerList[PlayerID];
                int shipID = _gameInstance.GameModel.NewShip(PlayerID);
                playerData.ShipID = shipID;

                StreamSocket clientSocket = await _networkConnection.ConnectToTCP(socket.Information.RemoteAddress.DisplayName, NetworkPorts.GameClientPort);
                playerData.ClientSocket = clientSocket;
                playerData.Connected = true;
            } 
        }

        private static int DistanceBetween(Ship ship, Entity entity)
        {
            return (int)(ship.Location - entity.Location).Length();
        }
    }
}
