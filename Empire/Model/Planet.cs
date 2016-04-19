using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire.Model
{
    // A very simple class to distinguish planets from other game entities.
    // Maybe someday planets will have populations, sovereignty, exports, etc.
    class Planet : Entity
    {
        public Planets planetID { get; private set; }

        public Planet(float x = 0, float y = 0, Planets ID = Planets.planet1) : base(x, y) {
            Location = new Vector2(x, y);
            planetID = ID;
            this.Type = EntityType.Planet;
        }

    }
}
