using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EmpireUWP.Model
{
    // A very simple class to distinguish planets from other game entities.
    // Maybe someday planets will have populations, sovereignty, exports, etc.
    public class Planet : Entity
    {
        public Planets PlanetID { get; set; }

        public Planet(GameModel gameModel, Planets ID = Planets.planet1) : base(gameModel) {
            PlanetID = ID;
        }

        public override void Initialize()
        {
            base.Initialize();
            this.Type = EntityType.Planet;
        }

        public override void SetState(ObjectState info)
        {
            base.SetState(info);
            PlanetID = (Planets)info.GetInt("PlanetID");
        }

        public override void GetState(ObjectState info)
        {
            base.GetState(info);
            info.AddValue("PlanetID", (int)PlanetID);
        }

        public override void HandleCollision(Entity entityThatCollided)
        {
            throw new NotImplementedException();  // at the moment, nothing can collide with a planet.  But if this gets called, it's a bug!
        }
    }
}
