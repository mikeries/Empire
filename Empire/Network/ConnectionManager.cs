using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Empire.Model;

namespace Empire.Network
{
    public static class ConnectionManager
    {
        private static Dictionary<string, Gamer> _gamerList = new Dictionary<string, Gamer>();
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

        public static void SendToHost(NetworkPacket packet)
        {
            NetworkInterface.SendPacket(NetworkInterface.HostEndPoint, packet);
        }

        public static void SendToAllGamers(NetworkPacket packet)
        {
            foreach (Gamer gamer in _gamerList.Values)
            {
                NetworkInterface.SendPacket(gamer.EndPoint, packet);
            }
        }

        private static void AddNewGamer(SalutationPacket salutation)
        {
            Gamer gamer = new Gamer(salutation.Name, salutation.Source);
            _gamerList.Add(salutation.Source.ToString(), gamer);
        }

        private static void ProcessIncomingPacket(object sender, PacketReceivedEventArgs e)
        {
            NetworkPacket packet = e.Packet;
            packet.Source = e.Source;

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

                case PacketType.TestPacket:
                    break;

                case PacketType.ShipCommand:
                    string sourceID = packet.Source.ToString();
                    GameModel.GetShip(sourceID).Command = packet as ShipCommand;
                    break;

                default:
                    // TODO: log unknown packet type
                    break;
            }
        }

        private static void ProcessAcknowledge(NetworkPacket packet)
        {
            // TODO: work on the joining stuff
            AcknowledgePacket ack = packet as AcknowledgePacket;
            if (ack.Response == AcknowledgePacket.Acknowledgement.OK)
            {
                ConnectionID = ack.ConnectionID;
            }
        }

        private static void ProcessSalutation(NetworkPacket packet)
        {
            if (NetworkInterface.IsHost)
            {
                SalutationPacket salutation = packet as SalutationPacket;
                Console.WriteLine("Salutation received from {0}", packet.Source);

                AcknowledgePacket ack = new AcknowledgePacket(salutation.Source.ToString(),salutation.Timestamp);
                ack.Response = DetermineAppropriateResponse(salutation);
                if (ack.Response == AcknowledgePacket.Acknowledgement.OK)
                {
                    AddNewGamer(salutation);
                }
                NetworkInterface.SendPacket(packet.Source, ack);
            }
        }

        private static AcknowledgePacket.Acknowledgement DetermineAppropriateResponse(SalutationPacket salutation)
        {
            AcknowledgePacket.Acknowledgement response = AcknowledgePacket.Acknowledgement.OK;
            if (_gamerList.ContainsKey(salutation.Source.ToString()))
            {
                response = AcknowledgePacket.Acknowledgement.MultipleConnectionFromOneEndPoint;
            }
            else if (_gamerList.Count >= MaximumPlayers)
            {
                response = AcknowledgePacket.Acknowledgement.GameFull;
            }
            else
            {
                foreach (Gamer gamer in _gamerList.Values)
                {
                    if (gamer.Name == salutation.Name)
                    {
                        //response = AcknowledgePacket.Acknowledgement.DuplicateName;
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


        internal static Ship GetShip()
        {
            return GameModel.GetShip(ConnectionID);
        }
    }
}
