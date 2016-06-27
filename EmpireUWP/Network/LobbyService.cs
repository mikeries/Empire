using EmpireUWP.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;

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

        private static Dictionary<string,Player> _playerList = new Dictionary<string, Player>();
        private static int _gameID = 0;
        private int NewGameID { get { return _gameID++; } }

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
                var players =
                    from player in _playerList.Values
                    where player.GameID != 0
                    select player.PlayerID;
                return players.ToList();
            }
        }

        public LobbyService(MenuManager menuManager)
        {
            Server = false;
            _menuManager = menuManager;
        }

        public async void InitializeAsync()
        {
            var hostNames = Windows.Networking.Connectivity.NetworkInformation.GetHostNames();
            _myAddress = hostNames.FirstOrDefault(name => name.Type == HostNameType.Ipv4).DisplayName;
            _lobbyConnection.PacketReceived += OnPacketReceived;
            if (_myAddress == lobbyAddress)
            {
                await _lobbyConnection.StartListening(lobbyPort);
                Server = true;
            }
        }

        public async void ConnectToLobbyAsync(string playerID)
        {
            await _lobbyConnection.ConnectAsync(lobbyAddress, lobbyPort);
            Player player = new Player(playerID);
            AddPlayer(player);
        }

        public void HostGame(string playerID)
        {

        }

        private void AddPlayer(Player player)
        {
            if (!Server)
            {
                _playerList.Add(player.PlayerID, player);
            }
            sendPlayerUpdateToServer(player);
        }

        private void sendPlayerUpdateToServer(Player player)
        {
            PlayerData packet = new PlayerData(player);
            _lobbyConnection.SendPacketToHost(packet);
        }
        
        internal void OnPacketReceived(object network, PacketReceivedEventArgs args)
        {
            StreamSocket source = args.Source;
            NetworkPacket packet = args.Packet;
            if (packet.Type == PacketType.PlayerData)
            {
                PlayerData data = packet as PlayerData;
                if (_playerList.ContainsKey(data.PlayerID))
                {
                    Player player = _playerList[data.PlayerID];
                    player.CopyFromPlayerData(data);
                    player.Location = source;
                }
                else
                {
                    Player player = new Player(data);
                    _playerList.Add(player.PlayerID, player);
                    player.Location = source;
                }

                _menuManager.PlayerListChanged();

                if (Server)
                {
                    EchoPacketToAllClients(packet);
                }
            }
        }

        private void EchoPacketToAllClients(NetworkPacket packet)
        {
            var destinationSockets =
                from player in _playerList.Values
                select player.Location;

            _lobbyConnection.SendPacketToAll(destinationSockets.ToList(), packet); 
        }

    }
}
