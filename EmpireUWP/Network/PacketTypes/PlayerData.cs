using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EmpireUWP.Network
{
    [DataContract]
    internal class PlayerData : NetworkPacket
    {
        [DataMember]
        internal string PlayerID { get; private set; }                   // the ID corresponding to this gamer
        [DataMember]
        internal int ShipID { get; set; }
        [DataMember]
        internal int Score { get; set; }
        [DataMember]
        internal int GameID { get; set; }

        internal PlayerData(Player player) : base()
        {
            Type = PacketType.PlayerData;
            PlayerID = player.PlayerID;
            ShipID = player.ShipID;
            Score = player.Score;
            GameID = player.GameID;
        }

    }
}