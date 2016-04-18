using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire.Model
{
    class Laser : Entity
    {
        const int Lifetime = 800; // how long a laser bullet lasts, in milliseconds

        public Laser()
        {
            this.timeToLive = Lifetime;
            this.Type = EntityType.Laser;
        }
    }
}
