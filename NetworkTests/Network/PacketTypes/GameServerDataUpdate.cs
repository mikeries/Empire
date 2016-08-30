using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTests
{
    [DataContract]
    public class GameServerDataUpdate : NetworkPacket
    {
        [DataMember]
        public Dictionary<string, PlayerData> PlayerList {get; private set;}
        [DataMember]
        public GameData GameData { get; private set; }

        public GameServerDataUpdate(Dictionary<string, PlayerData> playerList, GameData gameData)
        {
            Type = PacketType.GameServerDataUpdate;
            PlayerList = playerList;
            GameData = gameData;
        }
    }
}
