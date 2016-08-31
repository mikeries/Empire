
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Core;

namespace LobbyService
{
    public class LobbyService : INotifyPropertyChanged
    {
        private const string _serverAddress = "localhost";

        private PacketConnection _connection;

        private Dictionary<string,PlayerData> _playerList = new Dictionary<string, PlayerData>();
        private Dictionary<int, GameData> _gameList = new Dictionary<int, GameData>();

        public List<PlayerData> playerList { get { return _playerList.Values.ToList(); } }
        public List<GameData> gamesList { get { return _gameList.Values.ToList(); } }
        private string logText;
        public string LogText { get { return logText; } }

        private static int _gameIDSerialNumber = 1;
        private static int NewGameID { get { return _gameIDSerialNumber++; } }

        public LobbyService()
        {
            EmpireSerializer serializer = new EmpireSerializer();
            _connection = new PacketConnection(serializer);
        }

        private void log(string message)
        {
            logText += Environment.NewLine + message;
            OnPropertyChanged("LogText");
        }

        public async Task Initialize()
        {
            try
            {
                await _connection.StartTCPListener(NetworkPorts.LobbyServicePort, ProcessRequest);
                log("Service initialized.");
            } catch (Exception e)
            {
                log("Failed to start listener on port " + NetworkPorts.LobbyServicePort);
                log("HRESULT = " + e.HResult);
            }
         }

        private async Task<NetworkPacket> ProcessRequest(StreamSocket socket, NetworkPacket packet)
        {
            AcknowledgePacket acknowledgement = new AcknowledgePacket();

            if(packet.Type == PacketType.LobbyCommand)
            {
                LobbyCommandPacket command = packet as LobbyCommandPacket;
                string playerID = command.PlayerID;

                switch ((int)command.Command)
                {
                    case (int)LobbyCommands.EnterLobby:
                        string playerIPAddress = command.Data;
                        await ProcessEnterLobbyCommand(playerID, playerIPAddress);
                        break;
                    case (int)LobbyCommands.LeaveLobby:
                        await ProcessLeaveLobbyCommand(playerID);
                        break;
                    case (int)LobbyCommands.HostGame:
                        string hostIPAddress = command.Data;
                        await ProcessHostGameCommand(playerID, hostIPAddress);
                        break;
                    case (int)LobbyCommands.JoinGame:
                        string hostID = command.Data;
                        await ProcessJoinGameCommand(playerID, hostID);
                        break;
                    case (int)LobbyCommands.LeaveGame:
                        await ProcessLeaveGameCommand(playerID);
                        break;
                    case (int)LobbyCommands.SetupGame:
                        await ProcessSetupGameCommand(playerID);
                        break;
                }
                await UpdateAllClients();
                OnPropertyChanged("gamesList");
                OnPropertyChanged("playerList");
            }
            return acknowledgement;
        }

        private async Task ProcessLeaveGameCommand(string playerID)
        {
            int gameID = _playerList[playerID].GameID;

            if (gameID > 0)
            {
                _playerList[playerID].GameID = 0;
                GameData game = _gameList[gameID];

                game.Leave(playerID);
                if (game.PlayerCount == 0)
                {
                    _gameList.Remove(gameID);
                }
                else if (game.HostID == playerID)
                {
                    foreach (string id in game.playerList.ToList())
                    {
                        await SendLobbyCommandToClient(_playerList[id], LobbyCommands.LeaveGame);
                    }
                }
                log(playerID + " left his game.");
            }
        }

        private async Task ProcessJoinGameCommand(string playerID, string hostID)
        {
            if (playerID != hostID)
            {
                if (_playerList[playerID].GameID > 0)
                {
                    await ProcessLeaveGameCommand(playerID);
                }
                int gameID = _playerList[hostID].GameID;
                _gameList[gameID].Join(playerID);
                _playerList[playerID].GameID = gameID;
                log(playerID + " joined a game hosted by " + hostID + ".");
            }
        }

        private async Task ProcessHostGameCommand(string playerID, string hostIPAddress)
        {
            if (_playerList.ContainsKey(playerID))
            {
                if (_playerList[playerID].GameID != 0)
                {
                    await ProcessLeaveGameCommand(playerID);
                }

                GameData newGame = new GameData(NewGameID, playerID, hostIPAddress, NetworkPorts.GameServerRequestPort, NetworkPorts.GameServerUpdatePort);
                _playerList[playerID].GameID = newGame.GameID;
                _gameList.Add(newGame.GameID, newGame);
                log(playerID + " began hosting game #" + newGame.GameID + ".");
            }
        }

        private async Task ProcessLeaveLobbyCommand(string playerID)
        {
            if (_playerList.ContainsKey(playerID))
            {
                PlayerData player = _playerList[playerID];
                if (player.GameID != 0)
                {
                    await ProcessLeaveGameCommand(playerID);
                }
                _playerList.Remove(playerID);
                log(playerID + " left the lobby.");
            }
        }

        private async Task ProcessEnterLobbyCommand(string playerID, string ipAddress)
        {
            if (_playerList.ContainsKey(playerID))
            {
                await EjectUser(playerID);
                _playerList[playerID].IPAddress = ipAddress;
            } else 
            {
                PlayerData player = new PlayerData(new Player(playerID), ipAddress, NetworkPorts.GameClientUpdatePort);
                _playerList.Add(playerID, player);
            }
            log(playerID + " entered the lobby.");
        }

        private async Task EjectUser(string playerID)
        {
            // Send an command to the client to disconnect
            try
            {
                string address = _playerList[playerID].IPAddress;
                LobbyCommandPacket ejectUser = new LobbyCommandPacket(playerID, LobbyCommands.EjectThisUser);
                NetworkPacket reply = await _connection.ConnectAndWaitResponse(address, NetworkPorts.LobbyClientPort, ejectUser);
            } catch (Exception e)
            {
                // but if it doesn't work (possibly the client is disconnected) then we can safely ignore it and move on
                // the new client will use the old data.
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

            // request host to start listening
            if (data.HostID == playerID)
            {
                //await GamePage.gameInstance.StartServer(playerID, playerList, data);
            }

            // request each player to connect to host server
            foreach (PlayerData player in players)
            {
                await SendLobbyCommandToClient(player, LobbyCommands.EnterGame);
            }
            log(playerID + " pressed start.");
        }

        private async Task SendLobbyCommandToClient(PlayerData player, LobbyCommands command)
        {

            LobbyCommandPacket commandPacket = new LobbyCommandPacket(player.PlayerID, command);
            try
            {                
                await _connection.ConnectAndWaitResponse(player.IPAddress, NetworkPorts.LobbyClientPort, commandPacket);
            } catch (Exception e)
            {
                log("Failed to send command to remote client: HResult=" + e.HResult);
                log("Command: " + commandPacket.Command + " Player: " + player.PlayerID);
            }
        }

        private async Task UpdateAllClients()
        {
            LobbyData data = new LobbyData(_playerList, _gameList);

            var destinations =
                from player in _playerList.Values
                select player.IPAddress;

            foreach(string address in destinations)
            {
                try
                {
                    using (StreamSocket socket = await _connection.ConnectToTCP(address, NetworkPorts.LobbyClientPort))
                    {
                        await _connection.SendTCPData(socket, data);
                    }
                } catch (Exception e)
                {
                    log("An error occurred attempting to send data to " + address + ":" + NetworkPorts.LobbyClientPort);
                    log("HResult= "+e.HResult + "  This client will be removed.");
                    PlayerData disconnected = (PlayerData)_playerList.Values.Select((x) => { return (x.IPAddress == address); });
                    await ProcessLeaveLobbyCommand(disconnected.PlayerID);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public async void OnPropertyChanged(string propertyName)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                       () => { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); });
        }

    }
}
