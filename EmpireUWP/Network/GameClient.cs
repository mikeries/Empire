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
    public class GameClient
    {

        internal List<PlayerData> Gamers { get { return _playerList.Values.ToList(); } }
        internal string LocalPlayerID = null;
        internal bool Hosting
        {
            get
            {
                if (_gameData == null || LocalPlayerID == null)
                {
                    return false;
                }
                else
                {
                    return _gameData.HostID == LocalPlayerID;
                }
            }
        }

        private Dictionary<string, PlayerData> _playerList = new Dictionary<string, PlayerData>();
        private GameData _gameData;
        private GameView _gameInstance;
        private string _myAddress;
        private string _myPort;
        private UpdateQueue _updateQueue = new UpdateQueue();

        private PacketConnection _networkConnection;

        internal GameClient(GameView gameInstance, GameData gameData, string playerID, string port)
        {
            _gameInstance = gameInstance;
            _gameData = gameData;
            LocalPlayerID = playerID;
            var hostNames = Windows.Networking.Connectivity.NetworkInformation.GetHostNames();
            _myAddress = null;
            _myPort = port;
        }

        internal async Task CreateNetworkConnection()
        {
            if (_networkConnection == null)
            {
                EmpireSerializer serializer = new EmpireSerializer();
                _networkConnection = new PacketConnection(serializer);
                await _networkConnection.StartTCPListener(_myPort, HandleRequest);
                StreamSocket socket = await _networkConnection.ConnectToTCP(_gameData.HostIPAddress, _gameData.HostPort);
                _myAddress = socket.Information.LocalAddress.DisplayName;
            }
        }

        internal Task ConnectToServer()
        {
            NetworkPacket salutation = new Network.SalutationPacket(LocalPlayerID) as NetworkPacket;
            return SendRequestToHost(salutation);
        }

        private Task<NetworkPacket> SendRequestToHost(NetworkPacket packet)
        {
            return _networkConnection.ConnectAndWaitResponse(_gameData.HostIPAddress, _gameData.HostPort, packet);
        }

        internal Task UpdatePlayerStatus(PlayerData.PlayerStatus playerStatus)
        {
            PlayerData playerData = null;

            if (_playerList.ContainsKey(LocalPlayerID))
            {
                playerData = _playerList[LocalPlayerID];
            }
            else
            {
                playerData = new PlayerData(new Player(LocalPlayerID), _myAddress, _myPort);
            }
            playerData.Status = playerStatus;
            return SendRequestToHost(playerData);
        }

        internal void SendShipCommandToHost(ShipCommand shipCommand)
        {
            SendRequestToHost(shipCommand);
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
                    return _gameInstance.GameModel.NullShip;
                }
            }

            if (_playerList.ContainsKey(owner))
            {
                if (_playerList[owner].ShipID == 0)
                {
                    _playerList[owner].ShipID = _gameInstance.GameModel.NewShip(owner);
                }

                int shipID = _playerList[owner].ShipID;
                return _gameInstance.GameModel.GetEntity(shipID) as Ship;
            }

            return _gameInstance.GameModel.NullShip;
        }

        internal UpdateQueue RetrieveUpdatesAndClear()
        {
            UpdateQueue queue = _updateQueue;
            _updateQueue = new UpdateQueue();
            return queue;
        }

        private async Task<NetworkPacket> HandleRequest(StreamSocket socket, NetworkPacket packet)
        {
            AcknowledgePacket acknowledge = new Network.AcknowledgePacket();
            switch (packet.Type)
            {
                case PacketType.Entity:
                    _updateQueue.Add(packet as EntityPacket);
                    break;
                case PacketType.GameServerDataUpdate:
                    GameServerDataUpdate update = packet as GameServerDataUpdate;
                    _playerList = update.PlayerList;
                    _gameData = update.GameData;
                    break;
                case PacketType.ShipCommand:
                    _gameInstance.GameModel.InputManager.ProcessRemoteInput(packet as ShipCommand);
                    break;
            }
            await Task.Delay(0);
            return acknowledge;
        }

        internal event EventHandler<PacketReceivedEventArgs> PacketReceived = delegate { };
        private void OnPacketReceived(NetworkPacket packet)
        {
            PacketReceived?.Invoke(this, new PacketReceivedEventArgs(packet));
        }

        internal event EventHandler<GameChangedEventArgs> GameChanged = delegate { };
        private void OnGameChanged(GameData data)
        {
            GameChanged?.Invoke(this, new GameChangedEventArgs(data));
        }
    }
}
