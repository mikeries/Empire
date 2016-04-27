using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire
{

    // Circle structure -- used primarily for collision detection
    internal struct Circle
    {
        internal Vector2 Center { get; set; }
        internal float Radius { get; set; }

        internal Circle(Vector2 center, float radius)
        {
            Center = center;
            Radius = radius;
        }
    }
}
