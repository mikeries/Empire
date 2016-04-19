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

        private const int maxVelocity = 2;  // this velocity is added to the velocity of the parent when a child is created
        private const int minSizePercent = 20; // minSizePercent and maxSizePercent are used to scale a child relative to the parent size
        private const int maxSizePercent = 60;

        internal Asteroid(float x, float y) : base (x, y) {
            this.Status = Status.New;
            this.Orientation = (float)GameModel.Random.Next(0, 100);
            RollRate = GameModel.Random.Next(-300, 300) / 100f;
        }

        internal override void Update(GameTime gameTime)
        {
            this.Orientation += (float)(RollRate * Math.PI/18000*gameTime.ElapsedGameTime.TotalMilliseconds);
            base.Update(gameTime);
        }

        internal Asteroid childAsteroid()
        {
            Asteroid newAsteroid = new Asteroid(Location.X, Location.Y);

            float randomX = Velocity.X + (float)GameModel.Random.Next(-maxVelocity * 1000, maxVelocity * 1000) / 1000;
            float randomY = Velocity.Y + (float)GameModel.Random.Next(-maxVelocity * 1000, maxVelocity * 1000) / 1000;
            Vector2 vector = new Vector2(randomX, randomY);
            newAsteroid.Velocity = vector;
            int newSize = GameModel.Random.Next(minSizePercent, maxSizePercent) * Height / 100;
            if (newSize < 20)
            {
                newSize = 20;
            }
            newAsteroid.Height = newSize;
            newAsteroid.Width = newSize;
            newAsteroid.Stage = Stage - 1;
            newAsteroid.RollRate = GameModel.Random.Next(-(1000 / newSize), (1000 / newSize));
            newAsteroid.Style = Style;

            return newAsteroid;
        }

    }
}
