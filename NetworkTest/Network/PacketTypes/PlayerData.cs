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
    internal class PlayerData : NetworkPacket
    {
        [DataMember]
        internal string PlayerID { get; private set; }                   // the ID corresponding to this gamer
        [DataMember]
        internal string IPAddress { get; private set; }                  
        [DataMember]
        internal int ShipID { get; set; }
        [DataMember]
        internal int Score { get; set; }
        [DataMember]
        internal int GameID { get; set; }

        internal StreamSocket Location = null;

        internal PlayerData(Player player, string ipAddress) : base()
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