using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmpireUWP.Model;

namespace EmpireUWP.Network
{
    [Serializable]
    internal class ShipCommand : NetworkPacket
    {

        internal int Commands { get; private set; }
        internal bool Left { get { return (Commands & (int)CommandFlags.Left) > 0; } }
        internal bool Right { get { return ((Commands & (int)CommandFlags.Right) > 0); } }
        internal bool Shields { get { return ((Commands & (int)CommandFlags.Shields) > 0); } }
        internal bool Shoot { get { return ((Commands & (int)CommandFlags.Shoot) > 0); } }
        internal bool Thrust { get { return ((Commands & (int)CommandFlags.Thrust) > 0); } }
        internal string Owner { get; private set; }

        internal ShipCommand(string owner, int commands)
        {
            Type = PacketType.ShipCommand;
            Commands = commands;
            Owner = owner;
        }

    }
}
