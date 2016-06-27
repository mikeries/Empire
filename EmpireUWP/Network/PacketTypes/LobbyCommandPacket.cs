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
    internal class LobbyCommnandPacket : NetworkPacket
    {
        [DataMember]
        internal string PlayerID { get; private set; }                   // the ID corresponding to this gamer
        [DataMember]
        internal LobbyCommands Command { get; private set; }
        [DataMember]
        internal string Data { get; private set; }

        internal LobbyCommnandPacket(string playerID, LobbyCommands command, string args) : base()
        {
            Type = PacketType.LobbyCommand;
            PlayerID = playerID;
            Command = command;
            Data = args;
        }
    }
}
