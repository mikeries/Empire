using EmpireUWP.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

// TODO:  GameIDs and PlayerIDs should be custom types for better type checking and to hide implementation details.
namespace EmpireUWP.Network
{
    [DataContract]
    internal class GameData : NetworkPacket
    {
        internal enum GameStatus
        {
            WaitingForPlayers,
            ReadyToStart,
            InProgress,
            Ended,
            Paused,
        }
        [DataMember]
        internal int GameID { get; private set; }
        [DataMember]
        internal string HostID { get; private set; }
        [DataMember]
        internal List<string> playerList = new List<string>();
        internal int PlayerCount { get { return playerList.Count; } }
        [DataMember]
        internal GameStatus Status;

        internal GameData(int gameID, string playerID) : base()
        {
            Type = PacketType.GameData;
            HostID = playerID;
            GameID = gameID;
            Status = GameStatus.WaitingForPlayers;
            playerList.Add(playerID);
        }

        internal void Join(string playerID)
        {
            playerList.Add(playerID);
        }

        internal void Leave(string playerID)
        {
            if(playerList.Contains(playerID))
            {
                playerList.Remove(playerID);
            }
        }
    }
}
