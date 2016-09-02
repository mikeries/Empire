﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.IO;

namespace LobbyService
{
    [DataContract (Namespace="http://schemas.datacontract.org/2004/07/EmpireSerializer")]
    public class DeadEntitiesPacket : NetworkPacket
    {

        [DataMember]
        public List<int> EntityList;

        public DeadEntitiesPacket(List<int> entityList) : base()
        {
            Type = PacketType.DeadEntities;
            EntityList = entityList;
        }

    }
}
