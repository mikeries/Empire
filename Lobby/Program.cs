using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lobby
{
    class Program
    {
        static void Main(string[] args)
        {
            LobbyService server = new LobbyService();
            server.Start("http://localhost:1944/WebsocketHttpListenerDemo/");
            Console.WriteLine("Press a key to exit.");
            Console.ReadKey();
        }
    }
}
