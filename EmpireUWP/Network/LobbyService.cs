using EmpireUWP.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;

namespace EmpireUWP.Network
{
    public class LobbyService
    {
        private const string lobbyPort = "1944";
        private const string lobbyAddress = "192.168.1.245";
        private string _myAddress = null;
        internal bool Server { get; private set; }
        private NetworkConnection _lobbyConnection = new NetworkConnection();
        private MenuManager _menuManager;
        public MenuManager MenuManager { set { _menuManager = value; } }

        private Dictionary<string,PlayerData> _playerList = new Dictionary<string, PlayerData>();
        private Dictionary<int, GameData> _gameList = new Dictionary<int, GameData>();

        private static int _gameIDSerialNumber = 1;
        private static int NewGameID { get { return _gameIDSerialNumber++; } }

        public List<String> availablePlayers
        {
            get
            {
                var players =
                    from player in _playerList.Values
                    where player.GameID == 0
                    select player.PlayerID;
                return players.ToList();
            }
        }

        public List<String> hostedGames
        {
            get
            {
                var games =
                    from game in _gameList.Values
                    where game.Status == GameData.GameStatus.WaitingForPlayers
                    select game.HostID;
                return games.ToList();
            }
        }

        public List<string> GameMembers(string playerID)
        {
            List<string> players = new List<string>();
            if (_playerList.ContainsKey(playerID))
            {
                int gameID = _playerList[playerID].GameID;
                if (_gameList.ContainsKey(gameID))
                {
                    players=_gameList[gameID].playerList;
                }
            }
            return players;
        }

        internal GameData GetGameData(int gameID)
        {
            if (_gameList.ContainsKey(gameID))
            {
                return _gameList[gameID];
            }
            return null;
        }

        internal PlayerData GetPlayerData(string playerID)
        {
            if (_playerList.ContainsKey(playerID))
            {
                return _playerList[playerID];
            }
            return null;
        }

        public LobbyService(MenuManager menuManager)
        {
            Server = false;
        }

        public async Task Initialize()
        {
            var hostNames = Windows.Networking.Connectivity.NetworkInformation.GetHostNames();
            _myAddress = hostNames.FirstOrDefault(name => name.Type == HostNameType.Ipv4).DisplayName;
            _lobbyConnection.PacketReceived += OnPacketReceived;
            if (_myAddress == lobbyAddress)
            {
                await _lobbyConnection.StartListeningAsync(lobbyPort);
                Server = true;
            }
            return;
        }

        public async Task ConnectToLobbyAsync(string playerID)
        {
            await _lobbyConnection.ConnectAsync(lobbyAddress, lobbyPort);
        }

        public void DisconnectFromLobby()
        {
            _lobbyConnection.Close();
        }

        public async Task EnterLobby(string playerID)
        {
            LobbyCommandPacket command = new LobbyCommandPacket(playerID, LobbyCommands.EnterLobby);
            await _lobbyConnection.SendPacketToHost(command);
        }

        public async Task LeaveLobby(string playerID)
        {
            LobbyCommandPacket command = new LobbyCommandPacket(playerID, LobbyCommands.LeaveLobby);
            await _lobbyConnection.SendPacketToHost(command);
        }

        public async Task HostGame(string playerID)
        {
            LobbyCommandPacket command = new LobbyCommandPacket(playerID, LobbyCommands.HostGame, _myAddress);
            await _lobbyConnection.SendPacketToHost(command);
        }

        public async Task JoinGame(string playerID, string hostID)
        {
            LobbyCommandPacket command = new LobbyCommandPacket(playerID, LobbyCommands.JoinGame, hostID);
            await _lobbyConnection.SendPacketToHost(command);
        }

        public async Task LeaveGame(string playerID)
        {
            LobbyCommandPacket command = new LobbyCommandPacket(playerID, LobbyCommands.LeaveGame);
            await _lobbyConnection.SendPacketToHost(command);
        }

        public async Task InitializeGame(string playerID)
        {
            LobbyCommandPacket command = new LobbyCommandPacket(playerID, LobbyCommands.SetupGame);
            await _lobbyConnection.SendPacketToHost(command);
        }

        internal async void OnPacketReceived(object network, PacketReceivedEventArgs args)
        {
            StreamSocket source = args.Source;
            NetworkPacket packet = args.Packet;

            if(packet.Type == PacketType.LobbyCommand && Server)
            {
                LobbyCommandPacket command = packet as LobbyCommandPacket;
                string playerID = command.PlayerID;

                switch((int)command.Command)
                {
                    case (int)LobbyCommands.EnterLobby:
                        ProcessEnterLobbyCommand(playerID, source.Information.RemoteAddress.DisplayName);
                        break;
                    case (int)LobbyCommands.LeaveLobby:
                        ProcessLeaveLobbyCommand(playerID);
                        break;
                    case (int)LobbyCommands.HostGame:
                        string hostIPAddress = command.Data;
                        ProcessHostGameCommand(playerID, hostIPAddress);
                        break;
                    case (int)LobbyCommands.JoinGame:
                        string hostID = command.Data;
                        ProcessJoinGameCommand(playerID, hostID);
                        break;
                    case (int)LobbyCommands.LeaveGame:
                        ProcessLeaveGameCommand(playerID);
                        break;
                    case (int)LobbyCommands.SetupGame:
                        await ProcessSetupGameCommand(playerID);
                        break;
                }
                await UpdateAllClients();
                MenuManager.PlayerListChanged();
            }
            else if (packet.Type == PacketType.LobbyData && !Server)
            {
                LobbyData data = packet as LobbyData;
                _playerList = data._playerList;
                _gameList = data._gameList;
                MenuManager.PlayerListChanged();
            }
        }

        private void ProcessLeaveGameCommand(string playerID)
        {
            int gameID = _playerList[playerID].GameID;
            _playerList[playerID].GameID = 0;

            GameData game = _gameList[gameID];

            game.Leave(playerID);
            if (game.PlayerCount == 0)
            {
                _gameList.Remove(gameID);
            }
        }

        private void ProcessJoinGameCommand(string playerID, string hostID)
        {
            int gameID = _playerList[hostID].GameID;
            _gameList[gameID].Join(playerID);
            _playerList[playerID].GameID = gameID;
        }

        private void ProcessHostGameCommand(string playerID, string hostIPAddress)
        {
            if (_playerList.ContainsKey(playerID))
            {
                if (_playerList[playerID].GameID != 0)
                {
                    ProcessLeaveGameCommand(playerID);
                }

                GameData newGame = new GameData(NewGameID, playerID);
                _playerList[playerID].GameID = newGame.GameID;
                _gameList.Add(newGame.GameID, newGame);
            }
        }

        private void ProcessLeaveLobbyCommand(string playerID)
        {
            if (_playerList.ContainsKey(playerID))
            {
                PlayerData player = _playerList[playerID];
                if (player.GameID != 0)
                {
                    ProcessLeaveGameCommand(playerID);
                }
                _playerList.Remove(playerID);
            }
        }

        private void ProcessEnterLobbyCommand(string playerID, string ipAddress)
        {
            if (!_playerList.ContainsKey(playerID))
            {
                PlayerData player = new PlayerData(new Player(playerID), ipAddress);
                _playerList.Add(playerID, player);
            }
            else
            {
                // TODO: need to handle case where we are logging in a second time, possibly after disconnecting.

            }
        }

        private async Task ProcessSetupGameCommand(string playerID)
        {
            int game = _playerList[playerID].GameID;
            GameData data = _gameList[game];     

            var players =
                from playerdata in _playerList.Values
                where playerdata.GameID == game
                select playerdata;

            Dictionary<string,PlayerData> playerList = players.ToDictionary(id => id.PlayerID);
            await GamePage.gameInstance.SetupConnections(playerID, playerList, data);
        }

        private async Task UpdateAllClients()
        {
            LobbyData data = new LobbyData(_playerList, _gameList);

            var destinationSockets =
                from player in _playerList.Values
                select player.Location;

            await _lobbyConnection.SendPacketToAll(destinationSockets.ToList(), data); 
        }

    }
}
