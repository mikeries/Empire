using Empire.Model;
using Empire.Network.PacketTypes;
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
    abstract class NetworkPacket
    {
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        internal PacketType Type;
        internal IPEndPoint SourceIP;
        internal DateTime Timestamp { get; }  // when the packet was sent.  Used for determining lag time and as a packet ID

        internal NetworkPacket()
        {
            Timestamp = DateTime.Now;
        }

        internal virtual bool IsValid()
        {
            return true;
        }

        internal bool IsNewerThan(NetworkPacket networkPacket)
        {
            return (this.Timestamp > networkPacket.Timestamp);
        }

        internal static NetworkPacket ConstructPacketFromMessage(byte[] message)
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
                log.Fatal("Deserialization exception", e);
            }

            return packet;
        }

        internal virtual byte[] CreateMessageFromPacket()
        {
            byte[] message=null;
            IFormatter formatter = new BinaryFormatter();

            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    formatter.Serialize(stream, this);
                    message = stream.ToArray();
                }
            }
            catch (SerializationException e)
            {
                log.Fatal("Serialization exception", e);
                throw new Exception(e.Message);
            }

            return message;
        }
    }
}
