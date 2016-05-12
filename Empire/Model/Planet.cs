using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Empire.Model
{
    // A very simple class to distinguish planets from other game entities.
    // Maybe someday planets will have populations, sovereignty, exports, etc.
    class Planet : Entity
    {
        public Planets PlanetID { get; set; }

        public Planet(Planets ID = Planets.planet1) : base() {
            PlanetID = ID;
        }

        internal override void Initialize()
        {
            base.Initialize();
            this.Type = EntityType.Planet;
        }

        internal override void SetState(ObjectState info)
        {
            base.SetState(info);
            PlanetID = (Planets)info.GetInt("PlanetID");
        }

        public override void GetState(ObjectState info)
        {
            base.GetState(info);
            info.AddValue("PlanetID", (int)PlanetID);
        }

        internal override void HandleCollision(Entity entityThatCollided)
        {
            throw new NotImplementedException();  // at the moment, nothing can collide with a planet.  But if this gets called, it's a bug!
        }
    }
}
