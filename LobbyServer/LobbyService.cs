
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace LobbyTest
{
    public class LobbyService
    {
        private const string _serverAddress = "127.0.0.1";

        private PacketConnection _connection;

        private Dictionary<string,PlayerData> _playerList = new Dictionary<string, PlayerData>();
        private Dictionary<int, GameData> _gameList = new Dictionary<int, GameData>();

        private static int _gameIDSerialNumber = 1;
        private static int NewGameID { get { return _gameIDSerialNumber++; } }

        public LobbyService()
        {
            EmpireSerializer serializer = new EmpireSerializer();
            _connection = new PacketConnection(serializer);
        }

        public async Task Initialize()
        {
            await _connection.StartTCPListener(NetworkPorts.LobbyServerPort, ProcessRequest);
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
            }
            return acknowledgement;
        }

        private async Task ProcessLeaveGameCommand(string playerID)
        {
            int gameID = _playerList[playerID].GameID;
            _playerList[playerID].GameID = 0;

            GameData game = _gameList[gameID];

            game.Leave(playerID);
            if (game.PlayerCount == 0)
            {
                _gameList.Remove(gameID);
            } else if (game.HostID == playerID)
            {
                foreach(string id in game.playerList)
                {
                    await SendLobbyCommandToClient(_playerList[id], LobbyCommands.LeaveGame);
                }
            }
        }

        private async Task ProcessJoinGameCommand(string playerID, string hostID)
        {
            if(_playerList[playerID].GameID > 0)
            {
                await ProcessLeaveGameCommand(playerID);
            }
            int gameID = _playerList[hostID].GameID;
            _gameList[gameID].Join(playerID);
            _playerList[playerID].GameID = gameID;
        }

        private async Task ProcessHostGameCommand(string playerID, string hostIPAddress)
        {
            if (_playerList.ContainsKey(playerID))
            {
                if (_playerList[playerID].GameID != 0)
                {
                    await ProcessLeaveGameCommand(playerID);
                }

                GameData newGame = new GameData(NewGameID, playerID, hostIPAddress, NetworkPorts.GameServerPort);
                _playerList[playerID].GameID = newGame.GameID;
                _gameList.Add(newGame.GameID, newGame);
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
            }
        }

        private async Task ProcessEnterLobbyCommand(string playerID, string ipAddress)
        {
            if (_playerList.ContainsKey(playerID))
            {
                // TODO: need to handle case where we are logging in a second time, possibly after disconnecting.
                // For now, simply tell the old client to close.
                LobbyCommandPacket ejectUser = new LobbyCommandPacket(playerID, LobbyCommands.EjectThisUser);
                string address = _playerList[playerID].IPAddress;
                NetworkPacket reply = await _connection.ConnectAndWaitResponse(address, NetworkPorts.LobbyClientPort , ejectUser);

            } else 
            {
                PlayerData player = new PlayerData(new Player(playerID), ipAddress, NetworkPorts.GameClientPort);
                _playerList.Add(playerID, player);
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

        }

        private async Task SendLobbyCommandToClient(PlayerData player, LobbyCommands command)
        {
            LobbyCommandPacket commandPacket = new LobbyCommandPacket(player.PlayerID, command);
            await _connection.ConnectAndWaitResponse(player.IPAddress, NetworkPorts.LobbyClientPort, commandPacket);
        }

        private async Task UpdateAllClients()
        {
            LobbyData data = new LobbyData(_playerList, _gameList);

            var destinations =
                from player in _playerList.Values
                select player.IPAddress;

            foreach(string address in destinations)
            {
                using (StreamSocket socket = await _connection.ConnectToTCP(address, NetworkPorts.LobbyClientPort))
                {
                    await _connection.SendTCPData(socket, data);
                }
            }
        }

    }
}
