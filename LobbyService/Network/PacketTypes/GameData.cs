
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

// TODO:  GameIDs and PlayerIDs should be custom types for better type checking and to hide implementation details.
namespace LobbyService
{
    [DataContract (Namespace="http://schemas.datacontract.org/2004/07/EmpireSerializer")]
    public class GameData : NetworkPacket
    {
        public enum GameStatus
        {
            WaitingForPlayers,
            ReadyToStart,
            InProgress,
            Ended,
            Paused,
        }
        [DataMember]
        public int GameID { get; private set; }
        [DataMember]
        public string HostIPAddress { get; set; }
        [DataMember]
        public string HostRequestPort { get; set; }
        [DataMember]
        public string HostUpdatePort { get; set; }
        [DataMember]
        public string HostID { get; private set; }
        [DataMember]
        public List<string> playerList = new List<string>();
        public int PlayerCount { get { return playerList.Count; } }
        [DataMember]
        public GameStatus Status;

        public GameData(int gameID, string playerID, string ipAddress, string requestPort, string updatePort) : base()
        {
            Type = PacketType.GameData;
            HostID = playerID;
            HostIPAddress = ipAddress;
            HostRequestPort = requestPort;
            HostUpdatePort = updatePort;
            GameID = gameID;
            Status = GameStatus.WaitingForPlayers;
            playerList.Add(playerID);
        }

        public void Join(string playerID)
        {
            playerList.Add(playerID);
        }

        public void Leave(string playerID)
        {
            if(playerList.Contains(playerID))
            {
                playerList.Remove(playerID);
            }
        }
    }
}
