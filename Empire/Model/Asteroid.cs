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

        internal int Stage { get; set; }  // track how many times this rock can split, also influences score value.
        internal float RollRate { get; set; }  // how rapidly it rotates
        internal int Style { get; set; }  // which type of asteroid this is... mostly texture now, but might also be linked to score?

        internal Asteroid(int x, int y) : base (x, y) {
            this.Status = Status.New;
            this.Orientation = (float)GameModel.Random.Next(0, 100);
            RollRate = GameModel.Random.Next(-300, 300) / 100f;
        }

        internal override void Update(GameTime gameTime)
        {
            this.Orientation += (float)(RollRate * Math.PI/18000*gameTime.ElapsedGameTime.TotalMilliseconds);
            base.Update(gameTime);
        }

    }
}
