using System;
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

namespace EmpireUWP.Network
{
    public class ConnectionManager
    {

        internal bool Hosting { get { return _gameData.HostID == LocalPlayerID; } }
        internal List<PlayerData> Gamers { get { return _playerList.Values.ToList(); } }
        internal string LocalPlayerID = null;
        internal string HostIPAddress = null;
        internal bool Connected { get; private set; }
        internal bool ReadyToStart { get { return (_gameData.Status == GameData.GameStatus.InProgress); } }
        internal NetworkConnection GetNetworkConnection { get { return _networkConnection; } }

        private Dictionary<string, PlayerData> _playerList;
        private GameData _gameData;
        private string _myAddress;

        private NetworkConnection _networkConnection;
        private GameModel _gameModel;

        internal ConnectionManager(string playerID)
        {
            LocalPlayerID = playerID;
            var hostNames = Windows.Networking.Connectivity.NetworkInformation.GetHostNames();
            _myAddress = hostNames.FirstOrDefault(name => name.Type == HostNameType.Ipv4).DisplayName;

        }

        internal void Initialize(GameModel gameModel)
        {
            _gameModel = gameModel;
        }

        private void HandleUpdate(byte[] data)
        {
            throw new NotImplementedException();
        }

        private Task<NetworkPacket> HandleRequest(NetworkPacket packet)
        {
            throw new NotImplementedException();
        }

        internal async Task StartHosting(Dictionary<string, PlayerData> players, GameData gameData)
        {
            _playerList = players;
            _gameData = gameData;

            if (Hosting)
            {

                if (_networkConnection == null)
                {
                    EmpireSerializer serializer = new Network.EmpireSerializer();
                    _networkConnection = new Network.NetworkConnection(serializer);
                    Initialize(_gameModel);
                }

                await _networkConnection.StartRequestListener(NetworkPorts.GameServerRequestPort, HandleRequest);
            }

            HostIPAddress = players[_gameData.HostID].IPAddress;

            Connected = true;
        }

        internal async Task NotifyHost(string ipAddress)
        {
            EmpireSerializer serializer = new EmpireSerializer();
            _networkConnection = new NetworkConnection(serializer);
            await _networkConnection.StartUpdateListener(NetworkPorts.GameClientUpdatePort, HandleUpdate);
            await _networkConnection.StartRequestListener(NetworkPorts.GameClientRequestPort, HandleRequest);

            HostIPAddress = ipAddress;
            GameData gameData = new GameData(1,LocalPlayerID);
            gameData.Status = GameData.GameStatus.WaitingForPlayers;
            using (StreamSocket socket = await _networkConnection.Connect(HostIPAddress, NetworkPorts.GameServerRequestPort))
            {
                await _networkConnection.WaitResponsePacket(socket, gameData);
            }

            Connected = true;
        }


        internal async Task SendSyncUpdatesToPlayer(string player, List<EntityPacket> updates)
        {
            if (!Hosting) return;

            PlayerData gamer = _playerList[player];

            foreach (EntityPacket update in updates)
            {
                await _networkConnection.SendUpdatePacket(gamer.Location, update);
            }
        }

        //internal async void OnPacketReceived(object network, PacketReceivedEventArgs args)

        //{
        //    StreamSocket source = args.Source;
        //    NetworkPacket packet = args.Packet;

        //    switch(packet.Type)
        //    {
        //        case PacketType.PlayerData:
        //            PlayerData data = packet as PlayerData;
        //            data.Location = source;
        //            if (_playerList.ContainsKey(data.PlayerID))
        //            {
        //                _playerList[data.PlayerID] = data;
        //            }
        //            else
        //            {
        //                _playerList.Add(data.PlayerID, data);
        //            }
        //            await SendPacketToAllClients(packet);

        //            if (Hosting)
        //            {
        //                // check if we're ready to start game
        //                // by seeing if all packets have a sourcesocket
        //                bool ReadyToStart = true;
        //                foreach(PlayerData player in _playerList.Values)
        //                {
        //                    if(player.Location == null)
        //                    {
        //                        ReadyToStart = false;
        //                    }
        //                }
        //                if (ReadyToStart)
        //                {
        //                    _gameData.Status = GameData.GameStatus.ReadyToStart;
        //                    await SendPacketToAllClients(_gameData);
        //                    OnGameChanged(_gameData);
        //                }

        //            }
        //            break;
        //        case PacketType.GameData:
        //            _gameData = packet as GameData;
        //            // raise game changed event;
        //            OnGameChanged(_gameData);
        //            break;
        //        default:
        //            break;
        //    }
        //}

        internal async Task SendPacketToAllClients(NetworkPacket packet)
        {
            if (!Hosting) return;

            var destinationSockets =
                from player in _playerList.Values
                select player.Location;
            
            foreach (StreamSocket socket in destinationSockets)
            {
                await _networkConnection.SendUpdatePacket(socket, packet);
            }

        }

        internal async Task SendShipCommand(ShipCommand shipCommand)
        {
            string hostIPAddress = _playerList[_gameData.HostID].IPAddress;
            StreamSocket socket = await _networkConnection.Connect(hostIPAddress, NetworkPorts.GameServerUpdatePort);
            await _networkConnection.SendUpdatePacket(socket, shipCommand);
        }

        internal Ship GetShip(string owner = null)
        {
            if (owner == null)
            {
                if (LocalPlayerID != null)
                {
                    owner = LocalPlayerID;
                }
                else
                {
                    return GameModel.NullShip;
                }
            }

            if (_playerList.ContainsKey(owner))
            {
                if (_playerList[owner].ShipID == 0)
                {
                    _playerList[owner].ShipID = _gameModel.NewShip(owner);
                }
                //if (!Host) log.Debug("Requested ship: " + owner);
                int shipID = _playerList[owner].ShipID;
                return _gameModel.GetEntity(shipID) as Ship;
            }
            //log.Warn("Unknown ship requested.  Owner = " + owner);
            return GameModel.NullShip;
        }

        internal event EventHandler<GameChangedEventArgs> GameChanged = delegate { };
        private void OnGameChanged(GameData data)
        {
            GameChanged?.Invoke(this, new GameChangedEventArgs(data));
        }
    }
}
