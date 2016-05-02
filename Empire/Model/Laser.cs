using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Empire.Model
{
    class Laser : Entity
    {
        const int Lifetime = 800; // how long a laser bullet lasts, in milliseconds
        internal string Owner;

        public Laser()  : base()
        {
            Initialize();
        }

        internal override void Initialize()
        {
            age = 0;
            timeToLive = Lifetime;
            Type = EntityType.Laser;
            visualState = VisualStates.Idle;
        }

        internal override void SetState(ObjectState info)
        {
            base.SetState(info);
            Owner = info.GetString("Owner");
        }

        public override void GetState(ObjectState info)
        {
            base.GetState(info);
            info.AddValue("Owner", Owner);
        }

        internal override void HandleCollision(Entity entityThatCollided)
        {
            Status = Status.Dead;
        }

    }
}
