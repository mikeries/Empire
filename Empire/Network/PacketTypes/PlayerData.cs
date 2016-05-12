using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Empire.Network
{
    [Serializable]
    internal class PlayerData : NetworkPacket
    {
        internal string ConnectionID { get; private set; }                   // the ID corresponding to this gamer
        internal int ShipID { get; set; }
        internal int Score { get; set; }
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