using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmpireUWP.Model;
using System.Runtime.Serialization;

namespace EmpireUWP.Network
{
    [DataContract (Namespace="http://schemas.datacontract.org/2004/07/EmpireSerializer")]
    public class ShipCommand : NetworkPacket
    {
        [DataMember]
        public int Commands { get; private set; }
        [DataMember]
        public bool Left { get { return (Commands & (int)CommandFlags.Left) > 0; } set { } }

        [DataMember]
        public bool Right { get { return ((Commands & (int)CommandFlags.Right) > 0); } set { } }


        [DataMember]
        public bool Shields
        {
            get { return ((Commands & (int)CommandFlags.Shields) > 0); }
            set { }
        }

        [DataMember]
        public bool Shoot
        {
            get { return ((Commands & (int)CommandFlags.Shoot) > 0); }
            set { }
        }

        [DataMember]
        public bool Thrust
        {
            get { return ((Commands & (int)CommandFlags.Thrust) > 0); }
            set { }
        }

        [DataMember]
        public string Owner { get; private set; }

        public ShipCommand(string owner, int commands)
        {
            Type = PacketType.ShipCommand;
            Commands = commands;
            Owner = owner;
        }

    }
}
