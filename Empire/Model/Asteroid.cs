using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Empire.Model
{
    class Asteroid : Entity
    {

        public int Stage { get; set; }  // track how many times this rock can split, also influences score value.
        public int rollRate { get; set; }
        public int Style { get; set; }  // which type of asteroid this is... mostly texture now, but might also be linked to score?

        public Asteroid(int x, int y) : base (x, y) {
            this.Status = Status.New;
            this.Orientation = (float)GameModel.Random.Next(0, 100);
            rollRate = GameModel.Random.Next(-300, 300);
        }

        public override void Update(GameTime gameTime)
        {
            this.Orientation += (float)(rollRate / 100f * Math.PI/18000*gameTime.ElapsedGameTime.TotalMilliseconds);
            base.Update(gameTime);
        }

    }
}
