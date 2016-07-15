using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmpireUWP.Network;
using System.Runtime.Serialization;
using EmpireUWP.View;

namespace EmpireUWP.Model
{

    class NullShip : Ship
    {
 
        internal NullShip(GameModel model) : base(model)
        {
            Location = new Vector2(View.GameView.PlayArea.Width / 2, View.GameView.PlayArea.Height / 2);
            Velocity = new Vector2(Stopped, 0);
            this.Type = EntityType.Ship;
            Height = 1;
            Width = 1;
            ShieldEnergy = 100;
            Command = new ShipCommand("", 0);
        }

    }

}
