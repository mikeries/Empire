using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;

namespace NetworkTests
{
    [DataContract (Namespace="http://schemas.datacontract.org/2004/07/EmpireSerializer")]
    public class PlayerData : NetworkPacket
    {
        public enum PlayerStatus
        {
            Initializing,
            WaitingToStart,
            Playing,
            Paused,
            Dead,
        }

        [DataMember]
        public string PlayerID { get; private set; }                   
        [DataMember]
        public string IPAddress { get; private set; }
        [DataMember]
        public string Port { get; private set; }
        [DataMember]
        public PlayerStatus Status { get; set; }
        [DataMember]
        public bool Connected { get; set; }
        [DataMember]
        public int ShipID { get; set; }
        [DataMember]
        public int Score { get; set; }
        [DataMember]
        public int GameID { get; set; }

        public PlayerData(Player player, string ipAddress, string port) : base()
        {
            Type = PacketType.PlayerData;
            IPAddress = ipAddress;
            Port = port;
            Status = PlayerStatus.Initializing;
            Connected = false;
            PlayerID = player.PlayerID;
            ShipID = player.ShipID;
            Score = player.Score;
            GameID = player.GameID;
        }

    }
}