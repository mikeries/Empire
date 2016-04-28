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
    [Serializable]
    class Planet : Entity, ISerializable
    {
        public Planets PlanetID { get; private set; }

        public Planet(Vector2 location, Planets ID = Planets.planet1) : base(location) {
            Location = location;
            PlanetID = ID;
            this.Type = EntityType.Planet;
        }

        internal Planet(SerializationInfo info, StreamingContext context) : base(info,context)
        {
            PlanetID = (Planets)info.GetValue("PlanetID", typeof(int));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("PlanetID", PlanetID);
        }

    }
}
