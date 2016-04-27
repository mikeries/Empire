using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire
{
    // The VisualStates enum is used by the model to communicate to the view what visual features of the sprite should be 
    // drawn.  These are factors of 2 so that each corresponds to a bit in the integer.  For example, if
    // Status = VisualStates.Thrusting | VisualStates.Fire
    // then the animation system will show the ship on fire with thrusters going.
    enum VisualStates : int
    {
        Idle = 1,
        Thrusting = 2,
        Shields = 4,
        Shooting = 8,   // for a muzzle flash -- not yet implemented
        Fire = 16,      // When the ship is damaged, show flames on the sprite. -- Not yet implemented
        Count,
    }
}
