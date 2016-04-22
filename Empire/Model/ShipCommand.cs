using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire.Model
{
    struct ShipCommand
    {
        internal KeyboardState KeyboardState;

        public ShipCommand(KeyboardState keyboardState)
        {
            KeyboardState = keyboardState;
        }
    }
}
