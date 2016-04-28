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
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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

        internal static void SendToHost(NetworkPacket packet)
        {
            NetworkInterface.SendPacket(NetworkInterface.HostEndPoint, packet);
        }

        internal static void SendToAllGamers(NetworkPacket packet)
        {
            foreach (Gamer gamer in _gamerList.Values.ToList())
            {
                NetworkInterface.SendPacket(gamer.EndPoint, packet);
            }
        }

        private static void AddNewGamer(SalutationPacket salutation)
        {
            Gamer gamer = new Gamer(salutation.Name, salutation.Source);
            _gamerList.Add(salutation.Name, gamer);
            GameModel.NewShip(gamer.ConnectionID);
            log.Info(gamer.Name + " has joined the game from " + salutation.Source);
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

                case PacketType.ShipCommand:
                    ShipCommand command = packet as ShipCommand;
                    Ship ship = GameModel.GetShip(command.Owner);
                    if (ship != null)
                    {
                        ship.Command = command;
                    }
                    break;

                default:
                    // ignore other packet types -- they may be for someone else
                    break;
            }
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


        internal static Ship GetShip()
        {
            return GameModel.GetShip(ConnectionID);
        }
    }
}
