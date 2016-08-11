using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EmpireUWP.Network
{
    [DataContract]
    public class LobbyData : NetworkPacket
    {
        [DataMember]
        public Dictionary<string, PlayerData> _playerList {get; private set;}
        [DataMember]
        public Dictionary<int, GameData> _gameList { get; private set; }

        public LobbyData(Dictionary<string, PlayerData> playerList, Dictionary<int, GameData> gameList)
        {
            Type = PacketType.LobbyData;
            _playerList = playerList;
            _gameList = gameList;
        }
    }
}
