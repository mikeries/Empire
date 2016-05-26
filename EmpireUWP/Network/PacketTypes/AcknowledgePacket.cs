using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EmpireUWP.Network
{
    [DataContract]
    class AcknowledgePacket : NetworkPacket
    {
        public enum Acknowledgement : int
        {
            OK,
            DuplicateName,
            GameFull,
            MultipleConnectionFromOneEndPoint
        }
        [DataMember]
        public DateTime PacketToAcknowledge;
        [DataMember]
        public Acknowledgement Response;
        [DataMember]
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
