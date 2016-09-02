using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.IO;

namespace EmpireUWP.Network
{
    public class EmpireSerializer : ISerializer
    {
        private static Type[] _knownTypes = {
            typeof(AcknowledgePacket),
            typeof(EntityPacket),
            typeof(NetworkPacket),
            typeof(PingPacket),
            typeof(PlayerData),
            typeof(GameData),
            typeof(LobbyData),
            typeof(LobbyCommandPacket),
            typeof(SalutationPacket),
            typeof(ShipCommand),
            typeof(GameServerDataUpdate),
        };

        private DataContractSerializer _serializer = new DataContractSerializer(typeof(NetworkPacket), _knownTypes);

        public byte[] CreateMessageFromPacket(NetworkPacket packet)
        {
            byte[] message = null;

            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    _serializer.WriteObject(stream, packet);
                    message = stream.ToArray();
                }
            }
            catch (SerializationException e)
            {
                //log.Fatal("Serialization exception", e);
                throw new Exception(e.Message);
            }

            return message;
        }

        public NetworkPacket ConstructPacketFromMessage(byte[] message)
        {
            NetworkPacket packet = null;

            try
            {
                using (MemoryStream stream = new MemoryStream(message))
                {
                    packet = (NetworkPacket)_serializer.ReadObject(stream);
                }
            }
            catch (Exception)
            {
                return packet;  // null packet
            }

            return packet;
        }

    }
}