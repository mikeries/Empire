using EmpireUWP.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace EmpireUWP.Network
{
    public class LobbyService
    {
        private const string lobbyDataPort = "1944";
        private const string lobbyCommandPort = "1945";
        private const string lobbyAddress = "192.168.1.245";
        private string _myAddress = null;
        internal bool Server { get; private set; }
        private NetworkConnection _lobbyDataConnection = new NetworkConnection();
        private NetworkConnection _lobbyCommandConnection = new NetworkConnection();
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
            _lobbyDataConnection.PacketReceived += OnPacketReceived;
            if (_myAddress == lobbyAddress)
            {
                await _lobbyDataConnection.StartListeningAsync(lobbyDataPort);
                Server = true;
            }

            _lobbyCommandConnection.PacketReceived += OnRequestReceived;
            await _lobbyCommandConnection.StartListeningAsync(lobbyCommandPort);

            return;
        }

        public async Task ConnectToLobbyAsync(string playerID)
        {
            await _lobbyDataConnection.ConnectAsync(lobbyAddress, lobbyDataPort);
            //await _lobbyCommandConnection.ConnectAsync(lobbyAddress, lobbyCommandPort);
        }

        public void DisconnectFromLobby()
        {
            _lobbyDataConnection.Close();
            _lobbyCommandConnection.Close();
        }

        public async Task EnterLobby(string playerID)
        {
            LobbyCommandPacket command = new LobbyCommandPacket(playerID, LobbyCommands.EnterLobby);
            await _lobbyCommandConnection.WaitResponse(command);
        }

        public Task LeaveLobby(string playerID)
        {
            LobbyCommandPacket command = new LobbyCommandPacket(playerID, LobbyCommands.LeaveLobby);
            return _lobbyDataConnection.SendPacketToHost(command);
        }

        public Task HostGame(string playerID)
        {
            LobbyCommandPacket command = new LobbyCommandPacket(playerID, LobbyCommands.HostGame, _myAddress);
            return _lobbyDataConnection.SendPacketToHost(command);
        }

        public Task JoinGame(string playerID, string hostID)
        {
            LobbyCommandPacket command = new LobbyCommandPacket(playerID, LobbyCommands.JoinGame, hostID);
            return _lobbyDataConnection.SendPacketToHost(command);
        }

        public Task LeaveGame(string playerID)
        {
            LobbyCommandPacket command = new LobbyCommandPacket(playerID, LobbyCommands.LeaveGame);
            return _lobbyDataConnection.SendPacketToHost(command);
        }

        public Task InitializeGame(string playerID)
        {
            LobbyCommandPacket command = new LobbyCommandPacket(playerID, LobbyCommands.SetupGame);
            return _lobbyDataConnection.SendPacketToHost(command);
        }

        internal async void OnPacketReceived(object network, PacketReceivedEventArgs args)
        {
            StreamSocket source = args.Source;
            NetworkPacket packet = args.Packet;

            if(packet.Type == PacketType.LobbyCommand)
            {
                LobbyCommandPacket command = packet as LobbyCommandPacket;
                string playerID = command.PlayerID;

                if (Server)
                {
                    switch ((int)command.Command)
                    {
                        case (int)LobbyCommands.EnterLobby:
                            await ProcessEnterLobbyCommand(playerID, source.Information.RemoteAddress.DisplayName);
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
                } else
                {
                    switch ((int)command.Command)
                    {
                        case (int)LobbyCommands.EjectThisUser:
                            ProcessEjectThisUserCommand();
                            break;
                    }
                }
            }
            else if (packet.Type == PacketType.LobbyData && !Server)
            {
                LobbyData data = packet as LobbyData;
                _playerList = data._playerList;
                _gameList = data._gameList;
                MenuManager.PlayerListChanged();
            }

        }


        private async void OnRequestReceived(object sender, PacketReceivedEventArgs e)
        {
            StreamSocket source = e.Source;
            NetworkPacket packet = e.Packet;

            if (packet.Type == PacketType.LobbyCommand)
            {
                LobbyCommandPacket command = packet as LobbyCommandPacket;
                string playerID = command.PlayerID;
                AcknowledgePacket response = new AcknowledgePacket();

                await ProcessEnterLobbyCommand(playerID, source.Information.RemoteAddress.DisplayName);
                DataWriter writer = new DataWriter(source.OutputStream);

                writer.WriteInt32(5);
                await writer.StoreAsync();
                await writer.FlushAsync();
                writer.DetachStream();
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

        private async Task ProcessEnterLobbyCommand(string playerID, string ipAddress)
        {
            if (!_playerList.ContainsKey(playerID))
            {
                PlayerData player = new PlayerData(new Player(playerID), ipAddress);
                _playerList.Add(playerID, player);
            }
            else
            {
                // TODO: need to handle case where we are logging in a second time, possibly after disconnecting.
                LobbyCommandPacket ejectUser = new LobbyCommandPacket(playerID, LobbyCommands.EjectThisUser);
                await _lobbyDataConnection.SendPacketToHost(ejectUser);
            }
        }

        private Task ProcessSetupGameCommand(string playerID)
        {
            int game = _playerList[playerID].GameID;
            GameData data = _gameList[game];     

            var players =
                from playerdata in _playerList.Values
                where playerdata.GameID == game
                select playerdata;

            Dictionary<string,PlayerData> playerList = players.ToDictionary(id => id.PlayerID);
            return GamePage.gameInstance.SetupConnections(playerID, playerList, data);
        }

        private void ProcessEjectThisUserCommand()
        {
            // TODO:  Make this exit more graceful.  Perhaps bump user back to login page
            App.Current.Exit();
        }

        private Task UpdateAllClients()
        {
            LobbyData data = new LobbyData(_playerList, _gameList);

            var destinationSockets =
                from player in _playerList.Values
                select player.Location;

            return _lobbyDataConnection.SendPacketToAll(destinationSockets.ToList(), data); 
        }

    }
}
