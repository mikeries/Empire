﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTests
{
    public struct NetworkPorts
    {
        public static string LobbyServerRequestPort = "1945";
        public static string LobbyServerUpdatePort = "1946";
        public static string LobbyClientRequestPort = "1943";
        public static string LobbyClientUpdatePort = "1944";
        public static string GameServerRequestPort = "5555";
        public static string GameServerUpdatePort = "5556";
        public static string GameClientRequestPort = "5557";
        public static string GameClientUpdatePort = "5558";
    }
}
