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
        internal Dictionary<string, Player> _playerList = new Dictionary<string, Player>();
        internal bool Host { get; private set; }
        internal List<Player> Gamers { get { return _playerList.Values.ToList(); } }
        internal string PlayerID = null;
        internal bool Connected {
            get
            {
                if (PlayerID == null)
                    return false;
                else
                    return true;
            }
        }
        internal NetworkConnection GetNetworkConnection { get { return _networkConnection; } }

        private const int MaximumPlayers = 5;
        private string _myAddress;
        private const string _hostPort = "1967";
        private NetworkConnection _networkConnection;

        public ConnectionManager()
        {
            var hostNames = Windows.Networking.Connectivity.NetworkInformation.GetHostNames();
            _myAddress = hostNames.FirstOrDefault(name => name.Type == HostNameType.Ipv4).DisplayName;
            _networkConnection = new NetworkConnection();
            _networkConnection.PacketReceived += OnPacketReceived;
        }

        public async Task Join(string playerID, string hostAddress, string hostPort)
        {
            // send a request to the designated host to join
            PlayerID = playerID;
            Player player = new Player(PlayerID);
            PlayerData packet = new PlayerData(player);
            await _networkConnection.ConnectAsync(hostAddress, hostPort);
            _networkConnection.SendPacketToHost(packet);
        }

        public void Leave()
        {
            // leave game -- need to remember to handle host disconnection
        }

        public async void HostGame()
        {
            // set self up as host and start listening
            await _networkConnection.StartListening(_hostPort);
            Host = true;
        }

        internal void SendSyncUpdatesToPlayer(string player, List<EntityPacket> updates)
        {
            if (!Host) return;

            Player gamer = _playerList[player];

            foreach (EntityPacket update in updates)
            {
                _networkConnection.SendPacketTo(gamer.Location, update);
            }
        }

        private void AddNewGamer(SalutationPacket salutation)
        {
            //Gamer gamer = new Gamer(salutation.Name, salutation.SourceIP);
            //Gamers.TryAdd(salutation.Name, gamer);
            //gamer.ShipID = GameModel.NewShip(gamer.ConnectionID);
            //log.Info(gamer.ConnectionID + " has joined the game from " + salutation.SourceIP);
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

                if (Host)
                {
                    SendPacketToAllClients(packet);
                }
            }
        }

        internal void SendPacketToAllClients(NetworkPacket packet)
        {
            if (!Host) return;

            var destinationSockets =
                from player in _playerList.Values
                select player.Location;

            _networkConnection.SendPacketToAll(destinationSockets.ToList(), packet);
        }

        internal void IncreaseScore(string gamer, int v)
        {
            _playerList[gamer].Score += v;
        }

        internal void SendShipCommand(ShipCommand shipCommand)
        {
            _networkConnection.SendPacketToHost(shipCommand);
        }

        internal Ship GetShip(string owner = null)
        {
            if(owner == null)
            {
                if (PlayerID != null)
                {
                    owner = PlayerID;
                }
                else
                {
                    return GameModel.NullShip;
                }
            }

            if (_playerList.ContainsKey(owner))
            {
                int shipID = _playerList[owner].ShipID;
                //if (!Host) log.Debug("Requested ship: " + owner);
                return GameModel.GetEntity(shipID) as Ship;
            }
            //log.Warn("Unknown ship requested.  Owner = " + owner);
            return GameModel.NullShip;
        }
    }
}
