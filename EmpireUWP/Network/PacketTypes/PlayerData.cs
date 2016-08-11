using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;

namespace EmpireUWP.Network
{
    [DataContract]
    public class PlayerData : NetworkPacket
    {
        [DataMember]
        public string PlayerID { get; private set; }                   // the ID corresponding to this gamer
        [DataMember]
        public string IPAddress { get; private set; }                  
        [DataMember]
        public int ShipID { get; set; }
        [DataMember]
        public int Score { get; set; }
        [DataMember]
        public int GameID { get; set; }

        public PlayerData(Player player, string ipAddress) : base()
        {
            Type = PacketType.PlayerData;
            IPAddress = ipAddress;
            PlayerID = player.PlayerID;
            ShipID = player.ShipID;
            Score = player.Score;
            GameID = player.GameID;
        }

    }
}