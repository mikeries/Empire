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
        internal string ConnectionID { get; private set; }                   // the ID corresponding to this gamer
        [DataMember]
        internal int ShipID { get; set; }
        [DataMember]
        internal int Score { get; set; }
        [DataMember]
        internal IPEndPoint EndPoint { get; set; }

        internal PlayerData(Gamer player) : base()
        {
            Type = PacketType.PlayerData;
            ConnectionID = player.ConnectionID;
            ShipID = player.ShipID;
            Score = player.Score;
            EndPoint = player.EndPoint;
        }

    }
}