using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmpireUWP
{
    // CanvasLayer is used to determine plot order for sprites on the canvas
    // lower numbers will appear to be behind higher numbers
    enum CanvasLayer : int
    {
        Planet = 0,
        Asteroid = 1,
        Laser = 2,
        Ship = 3,
        Count = 4,
    }
}
