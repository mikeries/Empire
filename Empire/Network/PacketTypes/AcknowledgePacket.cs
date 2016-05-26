using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmpireUWP.Network
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
        public string Name;            

        public AcknowledgePacket(string name, DateTime packetToAcknowledge, Acknowledgement response=Acknowledgement.OK) : base()
        {
            PacketToAcknowledge = packetToAcknowledge;
            Response = response;
            Name = name;
            Type = PacketType.Acknowledge;
        }

    }
}
