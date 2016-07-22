using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmpireUWP
{

    // Circle structure -- used primarily for collision detection
    public struct Circle
    {
        public Vector2 Center { get; set; }
        public float Radius { get; set; }

        public Circle(Vector2 center, float radius)
        {
            Center = center;
            Radius = radius;
        }
    }
}
