using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Empire.Model
{
    [Serializable]
    class Laser : Entity, ISerializable
    {
        const int Lifetime = 800; // how long a laser bullet lasts, in milliseconds
        internal string Owner;

        public Laser(string owner) : base(new Microsoft.Xna.Framework.Vector2(0,0))
        {
            this.timeToLive = Lifetime;
            this.Type = EntityType.Laser;
            Owner = owner;
        }

        internal Laser(SerializationInfo info, StreamingContext context) : base(info,context)
        {
            Owner = (string)info.GetValue("Owner", typeof(string));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Owner", Owner);
        }

        internal override void HandleCollision(Entity entityThatCollided)
        {
            Status = Status.Dead;
        }

    }
}
