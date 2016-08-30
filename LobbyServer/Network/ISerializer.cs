using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobbyTest
{
    public interface ISerializer
    {
        NetworkPacket ConstructPacketFromMessage(byte[] message);
        byte[] CreateMessageFromPacket(NetworkPacket packet);
    }
}
