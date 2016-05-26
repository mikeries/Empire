using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EmpireUWP.Model;
using EmpireUWP.View;
using System.Collections.Concurrent;

namespace EmpireUWP.Network
{
    public static class ConnectionManager
    {
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger("ConnectionManager");

        internal static ConcurrentDictionary<string, Gamer> Gamers = new ConcurrentDictionary<string, Gamer>();

        private const int MaximumPlayers = 5;
        public static bool IsHost { get { return NetworkInterface.IsHost; } }
        public static string ConnectionID { get; private set; }

        static ConnectionManager()
        {
            ConnectionID = null;
        }

        public static void Join(string name)
        {
            SalutationPacket requestToJoin = new SalutationPacket(name);
            SendToHost(requestToJoin);
        }

        public static void Leave()
        {

        }

        public static void HostGame()
        {

        }

        public static void Initialize()
        {
            NetworkInterface.Initialize();
            NetworkInterface.PacketReceived += ProcessIncomingPacket;
        }

        internal static void SendToHost(NetworkPacket packet)
        {
            NetworkInterface.SendPacket(NetworkInterface.HostEndPoint, packet);
        }

        internal static void SendToAllGamers(NetworkPacket packet)
        {
            foreach (Gamer gamer in Gamers.Values.ToList())
            {
                NetworkInterface.SendPacket(gamer.EndPoint, packet);
            }
        }

        internal static void SendSyncUpdatesToPlayer(string player, List<EntityPacket> updates)
        {
            Gamer gamer = Gamers[player];
            IPEndPoint destination = gamer.EndPoint;
            foreach(EntityPacket update in updates)
            {
                NetworkInterface.SendPacket(destination, update);
            }
        }

        internal static void SendPlayerDataToAll()
        {
            foreach(Gamer gamer in Gamers.Values.ToList())
            {
                PlayerData data = new PlayerData(gamer);
                SendToAllGamers(data);
            }
        }

        private static void AddNewGamer(SalutationPacket salutation)
        {
            Gamer gamer = new Gamer(salutation.Name, salutation.SourceIP);
            Gamers.TryAdd(salutation.Name, gamer);
            gamer.ShipID = GameModel.NewShip(gamer.ConnectionID);
            log.Info(gamer.ConnectionID + " has joined the game from " + salutation.SourceIP);
        }

        private static void ProcessIncomingPacket(object sender, PacketReceivedEventArgs e)
        {
            NetworkPacket packet = e.Packet;
            packet.SourceIP = e.SourceIP;

            switch (packet.Type)
            {
                case PacketType.Ping:
                    ProcessPing(packet);
                    break;

                case PacketType.Salutation:
                    ProcessSalutation(packet);
                    break;

                case PacketType.Acknowledge:
                    ProcessAcknowledge(packet);
                    break;

                case PacketType.PlayerData:
                    ProcessPlayerData(packet);
                    break;

                default:
                    // ignore other packet types -- they may be for someone else
                    break;
            }
        }

        private static void ProcessPlayerData(NetworkPacket packet)
        {
            PlayerData data = packet as PlayerData;
            Gamer gamer;
            if(Gamers.ContainsKey(data.ConnectionID))
            {
                gamer = Gamers[data.ConnectionID]; 
            }
            else
            {
                gamer = new Gamer(data.ConnectionID, data.EndPoint);
                Gamers.TryAdd(data.ConnectionID, gamer);
            }

            gamer.CopyFromPlayerData(data);
        }

        private static void ProcessAcknowledge(NetworkPacket packet)
        {
            // TODO: work on the joining stuff
            AcknowledgePacket ack = packet as AcknowledgePacket;
            if (ack.Response == AcknowledgePacket.Acknowledgement.OK)
            {
                ConnectionID = ack.Name;
            }
            else if (ack.Response == AcknowledgePacket.Acknowledgement.DuplicateName)
            {
                string newID = ack.Name + GameModel.Random.Next(0, 10);
                SalutationPacket newSalutation = new SalutationPacket(newID);
                SendToHost(newSalutation);
            }
        }

        private static void ProcessSalutation(NetworkPacket packet)
        {
            if (NetworkInterface.IsHost)
            {
                SalutationPacket salutation = packet as SalutationPacket;

                AcknowledgePacket ack = new AcknowledgePacket(salutation.Name,salutation.Timestamp);
                ack.Response = DetermineAppropriateResponse(salutation);
                if (ack.Response == AcknowledgePacket.Acknowledgement.OK)
                {
                    AddNewGamer(salutation);
                }
                NetworkInterface.SendPacket(packet.SourceIP, ack);
            }
        }

        internal static void IncreaseScore(string gamer, int v)
        {
            Gamers[gamer].Score += v;
        }

        private static AcknowledgePacket.Acknowledgement DetermineAppropriateResponse(SalutationPacket salutation)
        {
            AcknowledgePacket.Acknowledgement response = AcknowledgePacket.Acknowledgement.OK;
            if (Gamers.ContainsKey(salutation.SourceIP.ToString()))
            {
                response = AcknowledgePacket.Acknowledgement.MultipleConnectionFromOneEndPoint;
            }
            else if (Gamers.Count >= MaximumPlayers)
            {
                response = AcknowledgePacket.Acknowledgement.GameFull;
            }
            else
            {
                foreach (Gamer gamer in Gamers.Values)
                {
                    if (gamer.ConnectionID == salutation.Name)
                    {
                        response = AcknowledgePacket.Acknowledgement.DuplicateName;
                    }
                }
            }
            return response;
        }

        internal static void SendShipCommand(ShipCommand shipCommand)
        {
            SendToHost(shipCommand);
        }

        private static void ProcessPing(NetworkPacket packet)
        {
            PingPacket ping = packet as PingPacket;
        }

        internal static Ship GetShip(string owner = null)
        {
            if(owner == null)
            {
                owner = ConnectionID;
            }

            if (Gamers.ContainsKey(owner))
            {
                int shipID = Gamers[owner].ShipID;
                //if (!IsHost) log.Debug("Requested ship: " + owner);
                return GameModel.GetEntity(shipID) as Ship;
            }
            log.Warn("Unknown ship requested.  Owner = " + owner);
            return null;
        }
    }
}
