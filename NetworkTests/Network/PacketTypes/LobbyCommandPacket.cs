﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTests
{
    [DataContract]
    public class LobbyCommandPacket : NetworkPacket
    {
        [DataMember]
        public string PlayerID { get; private set; }                   // the ID corresponding to this gamer
        [DataMember]
        public LobbyCommands Command { get; private set; }
        [DataMember]
        public string Data { get; private set; }

        public LobbyCommandPacket(string playerID, LobbyCommands command, string args=null) : base()
        {
            Type = PacketType.LobbyCommand;
            PlayerID = playerID;
            Command = command;
            Data = args;
        }
    }
}
