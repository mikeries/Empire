using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EmpireUWP.Network
{
    public class Gamer
    {
        public string ConnectionID { get; private set; }                   // the ID corresponding to this gamer
        public IPEndPoint EndPoint { get; private set; }
        public int ShipID { get; set; }
        public int Score { get; set; }

        public Gamer(string name, IPEndPoint endpoint)
        {
            EndPoint = endpoint;
            ConnectionID = name;
            ShipID = 0;
            Score = 0;
        }

        internal void CopyFromPlayerData(PlayerData data)
        {
            ShipID = data.ShipID;
            Score = data.Score;
            EndPoint = data.EndPoint;
        }
    }
}
