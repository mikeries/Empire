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
        private Dictionary<string, PlayerData> _playerList;
        private GameData _gameData;
        internal bool Hosting { get { return _gameData.HostID == LocalPlayerID; } }
        internal List<PlayerData> Gamers { get { return _playerList.Values.ToList(); } }
        internal string LocalPlayerID = null;
        internal bool Connected { get; private set; }
        internal bool ReadyToStart { get { return (_gameData.Status == GameData.GameStatus.InProgress); } }
        internal NetworkConnection GetNetworkConnection { get { return _networkConnection; } }

        private const int MaximumPlayers = 5;
        private string _myAddress;
        private const string _hostPort = "5555";
        private NetworkConnection _networkConnection;
        private GameModel _gameModel;

        internal ConnectionManager(string playerID)
        {
            LocalPlayerID = playerID;
            var hostNames = Windows.Networking.Connectivity.NetworkInformation.GetHostNames();
            _myAddress = hostNames.FirstOrDefault(name => name.Type == HostNameType.Ipv4).DisplayName;
            _networkConnection = new NetworkConnection();
            _networkConnection.PacketReceived += OnPacketReceived;
        }

        internal void Initialize(GameModel gameModel)
        {
            _gameModel = gameModel;
        }

        internal async Task SetupGameConnectionsAsync(Dictionary<string,PlayerData> players, GameData gameData)
        {
            _playerList = players;
            _gameData = gameData;

            if (Hosting)
            {
                await _networkConnection.StartListeningAsync(_hostPort);
            }

            string hostIPAddress = players[_gameData.HostID].IPAddress;
            await _networkConnection.ConnectAsync(hostIPAddress, _hostPort);

            //_networkConnection.SendPacketToHost(_gameData);

            PlayerData packet = _playerList[LocalPlayerID];
            await _networkConnection.SendPacketToHost(packet);

            Connected = true;
        }

        internal async Task SendSyncUpdatesToPlayer(string player, List<EntityPacket> updates)
        {
            if (!Hosting) return;

            PlayerData gamer = _playerList[player];

            foreach (EntityPacket update in updates)
            {
                await _networkConnection.SendPacketTo(gamer.Location, update);
            }
        }

        internal async void OnPacketReceived(object network, PacketReceivedEventArgs args)

        {
            StreamSocket source = args.Source;
            NetworkPacket packet = args.Packet;

            switch(packet.Type)
            {
                case PacketType.PlayerData:
                    PlayerData data = packet as PlayerData;
                    data.Location = source;
                    if (_playerList.ContainsKey(data.PlayerID))
                    {
                        _playerList[data.PlayerID] = data;
                    }
                    else
                    {
                        _playerList.Add(data.PlayerID, data);
                    }
                    await SendPacketToAllClients(packet);

                    if (Hosting)
                    {
                        // check if we're ready to start game
                        // by seeing if all packets have a sourcesocket
                        bool ReadyToStart = true;
                        foreach(PlayerData player in _playerList.Values)
                        {
                            if(player.Location == null)
                            {
                                ReadyToStart = false;
                            }
                        }
                        if (ReadyToStart)
                        {
                            _gameData.Status = GameData.GameStatus.ReadyToStart;
                            await SendPacketToAllClients(_gameData);
                            OnGameChanged(_gameData);
                        }

                    }
                    break;
                case PacketType.GameData:
                    _gameData = packet as GameData;
                    // raise game changed event;
                    OnGameChanged(_gameData);
                    break;
                default:
                    break;
            }
        }

        internal async Task SendPacketToAllClients(NetworkPacket packet)
        {
            if (!Hosting) return;

            var destinationSockets =
                from player in _playerList.Values
                select player.Location;

            await _networkConnection.SendPacketToAll(destinationSockets.ToList(), packet);
        }

        internal async Task SendShipCommand(ShipCommand shipCommand)
        {
            await _networkConnection.SendPacketToHost(shipCommand);
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
