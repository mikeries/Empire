using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;

namespace LobbyTest
{
    public class Player
    {
        public string PlayerID { get; private set; }                   // the ID corresponding to this gamer
        public int GameID { get; set; }
        public int ShipID { get; set; }
        public int Score { get; set; }

        public Player(string name = "")
        {
            PlayerID = name;
            ShipID = 0;
            Score = 0;
            GameID = 0;
        }

        internal Player(PlayerData data)
        {
            CopyFromPlayerData(data);
        }

        internal void CopyFromPlayerData(PlayerData data)
        {
            ShipID = data.ShipID;
            Score = data.Score;
            GameID = data.GameID;
            PlayerID = data.PlayerID;
        }
    }
}
