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
    [DataContract]
    internal class ShipCommand : NetworkPacket
    {
        [DataMember]
        internal int Commands { get; private set; }
        [DataMember]
        internal bool Left { get { return (Commands & (int)CommandFlags.Left) > 0; } set { } }

        [DataMember]
        internal bool Right { get { return ((Commands & (int)CommandFlags.Right) > 0); } set { } }


        [DataMember]
        internal bool Shields
        {
            get { return ((Commands & (int)CommandFlags.Shields) > 0); }
            set { }
        }

        [DataMember]
        internal bool Shoot
        {
            get { return ((Commands & (int)CommandFlags.Shoot) > 0); }
            set { }
        }

        [DataMember]
        internal bool Thrust
        {
            get { return ((Commands & (int)CommandFlags.Thrust) > 0); }
            set { }
        }

        [DataMember]
        internal string Owner { get; private set; }

        internal ShipCommand(string owner, int commands)
        {
            Type = PacketType.ShipCommand;
            Commands = commands;
            Owner = owner;
        }

    }
}
