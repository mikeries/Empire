using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire.Network
{
    [Serializable]
    class AcknowledgePacket : NetworkPacket
    {
        public enum Acknowledgement : int
        {
            OK,
            DuplicateName,
            GameFull,
            MultipleConnectionFromOneEndPoint
        }
        public DateTime PacketToAcknowledge;
        public Acknowledgement Response;
        public string ConnectionID;            

        public AcknowledgePacket(string connectionID, DateTime packetToAcknowledge, Acknowledgement response=Acknowledgement.OK) : base()
        {
            PacketToAcknowledge = packetToAcknowledge;
            Response = response;
            ConnectionID = connectionID;
            Type = PacketType.Acknowledge;
        }

    }
}
