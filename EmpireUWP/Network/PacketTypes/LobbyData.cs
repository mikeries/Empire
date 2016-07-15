using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EmpireUWP.Network
{
    [DataContract]
    internal class LobbyData : NetworkPacket
    {
        [DataMember]
        internal Dictionary<string, PlayerData> _playerList {get; private set;}
        [DataMember]
        internal Dictionary<int, GameData> _gameList { get; private set; }

        internal LobbyData(Dictionary<string, PlayerData> playerList, Dictionary<int, GameData> gameList)
        {
            _playerList = playerList;
            _gameList = gameList;
        }
    }
}
