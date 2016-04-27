using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Empire.Network
{
    [Serializable]
    public abstract class NetworkPacket
    {
        public PacketType Type;
        public IPEndPoint Source;
        public DateTime Timestamp { get; private set; }  // when the packet was sent.  Used for determining lag time and as a packet ID
        
        public NetworkPacket()
        {
            Timestamp = DateTime.Now;
        }

        public virtual bool IsValid()
        {
            return true;
        }

        public static NetworkPacket ConstructPacketFromMessage(byte[] message)
        {
            NetworkPacket packet = null;
            IFormatter formatter = new BinaryFormatter();

            try
            {
                using (MemoryStream stream = new MemoryStream(message))
                {
                    packet = (NetworkPacket)formatter.Deserialize(stream);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return packet;
        }

        public virtual byte[] CreateMessageFromPacket()
        {
            byte[] message;
            IFormatter formatter = new BinaryFormatter();

            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, this);
                message = stream.ToArray();
            }

            return message;
        }
    }
}
