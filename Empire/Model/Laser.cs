using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire.Model
{
    class Laser : Entity
    {
        public Laser()
        {
            this.timeToLive = 800;
            this.Type = EntityType.Laser;
        }
    }
}
