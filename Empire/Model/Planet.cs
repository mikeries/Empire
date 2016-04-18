using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire.Model
{
    class Planet : Entity
    {
        public Planets planetID { get; private set; }

        public Planet(int x = 0, int y = 0, Planets ID = Planets.planet1) : base(x, y) {
            Location = new Vector2(x, y);
            planetID = ID;
            this.Type = EntityType.Planet;
        }

    }
}
