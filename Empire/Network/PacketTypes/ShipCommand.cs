using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Empire.Model;

namespace Empire.Network
{
    [Serializable]
    public class ShipCommand : NetworkPacket
    {
        
        public int Commands;
        public bool Left { get { return (Commands & (int)CommandFlags.Left) > 0; } }
        public bool Right { get { return ((Commands & (int)CommandFlags.Right) > 0); } }
        public bool Shields { get { return ((Commands & (int)CommandFlags.Shields) > 0); } }
        public bool Shoot { get { return ((Commands & (int)CommandFlags.Shoot) > 0); } }
        public bool Thrust { get { return ((Commands & (int)CommandFlags.Thrust) > 0); } }

        public ShipCommand(int commands)
        {
            Type = PacketType.ShipCommand;
            Commands = commands;
        }

    }
}
