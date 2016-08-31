
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Core;

namespace EmpireUWP.Network
{
    public class Lobby : INotifyPropertyChanged
    {

        private const string _serverAddress = "192.168.1.11";
        private string _myAddress = null;
        private PacketConnection _connection;

        private Dictionary<string, PlayerData> _playerList = new Dictionary<string, PlayerData>();
        private Dictionary<int, GameData> _gameList = new Dictionary<int, GameData>();
        
        public List<PlayerData> playerList {  get { return _playerList.Values.ToList(); } }
        public List<GameData> gamesList { get { return _gameList.Values.ToList(); } }

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
                    players = _gameList[gameID].playerList;
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

        public Lobby()
        {
            EmpireSerializer serializer = new EmpireSerializer();
            _connection = new PacketConnection(serializer);
        }

        public async Task Initialize()
        {
            if(_myAddress != null)
            {
                return;     // already been initialized.
            }

            try
            {
                using (StreamSocket socket = await _connection.ConnectToTCP(_serverAddress, NetworkPorts.LobbyServerPort))
                {
                    _myAddress = socket.Information.LocalAddress.DisplayName;
                }

                await _connection.StartTCPListener(NetworkPorts.LobbyClientPort, ProcessRequest);
            } catch (Exception e)
            {
                // Could not reach lobby.
                //TODO:  Create custom exceptions to be thrown by lobby and caught and handled by the gameView.
                LobbyCommandPacket packet = new LobbyCommandPacket("Client", LobbyCommands.Disconnected);
                OnLobbyCommand(packet);
            }

            return;
        }

        public async Task EnterLobby(string playerID)
        {
            NetworkPacket replyPacket = await SendLobbyCommand(playerID, LobbyCommands.EnterLobby, _myAddress);
        }

        public async Task LeaveLobby(string playerID)
        {
            NetworkPacket replyPacket = await SendLobbyCommand(playerID, LobbyCommands.LeaveLobby);
        }

        public async Task HostGame(string playerID)
        {
            NetworkPacket replyPacket = await SendLobbyCommand(playerID, LobbyCommands.HostGame, _myAddress);
        }

        public async Task JoinGame(string playerID, string hostID)
        {
            NetworkPacket replyPacket = await SendLobbyCommand(playerID, LobbyCommands.JoinGame, hostID);
        }

        public async Task LeaveGame(string playerID)
        {
            NetworkPacket replyPacket = await SendLobbyCommand(playerID, LobbyCommands.LeaveGame);
        }

        public async Task InitializeGame(string playerID)
        {
            NetworkPacket replyPacket = await SendLobbyCommand(playerID, LobbyCommands.SetupGame);
        }

        public async Task StartGame(string playerID)
        {
            GameData gameData = _gameList[_playerList[playerID].GameID];
            //await GamePage.gameInstance.StartGame(playerID, gameData);
        }

        private async Task<NetworkPacket> SendLobbyCommand(string playerID, LobbyCommands command, string args = null)
        {
            try
            {
                LobbyCommandPacket commandPacket = new LobbyCommandPacket(playerID, command, args);
                return await _connection.ConnectAndWaitResponse(_serverAddress, NetworkPorts.LobbyServerPort, commandPacket);
            } catch (Exception e)
            {
                // TODO throw custom exception for gameview to deal with
                // for now, fire LobbyCommand.Disconnected.
                LobbyCommandPacket packet = new LobbyCommandPacket("Client", LobbyCommands.Disconnected);
                OnLobbyCommand(packet);
            }

            return null as NetworkPacket;
        }

        private async Task<NetworkPacket> ProcessRequest(StreamSocket socket, NetworkPacket packet)
        {
            AcknowledgePacket acknowledgement = new AcknowledgePacket();

            if (packet.Type == PacketType.LobbyCommand)
            {
                LobbyCommandPacket command = packet as LobbyCommandPacket;
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () => { OnLobbyCommand(command); });
            } else if (packet.Type == PacketType.LobbyData)
            {
                LobbyData lobbyData = packet as LobbyData;
                _playerList = lobbyData._playerList;
                _gameList = lobbyData._gameList;
                OnPropertyChanged("playerList");
                OnPropertyChanged("gamesList");
            }
            return acknowledgement;
        }

        internal event EventHandler<LobbyCommandEventArgs> LobbyCommand = delegate { };
        public void OnLobbyCommand(LobbyCommandPacket command)
        {
            LobbyCommand?.Invoke(this, new LobbyCommandEventArgs(command));
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public async void OnPropertyChanged(string propertyName)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                       () => { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); });
        }
    }
}
