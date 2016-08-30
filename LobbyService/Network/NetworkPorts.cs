using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobbyService
{
    public struct NetworkPorts
    {
        public static string LobbyServicePort = "1945";
        public static string LobbyServiceUpdatePort = "1946";
        public static string LobbyClientPort = "1943";
        public static string LobbyClientUpdatePort = "1944";
        public static string GameServerRequestPort = "5555";
        public static string GameServerUpdatePort = "5556";
        public static string GameClientRequestPort = "5557";
        public static string GameClientUpdatePort = "5558";
    }
}
