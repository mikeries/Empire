using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using System.Runtime.Serialization;

namespace Empire.Model
{
    [Serializable]
    class Asteroid : Entity, ISerializable
    {
        internal int Stage { get; set; }  // track how many times this rock can split, also influences score value.
        internal float RollRate { get; set; }  // how rapidly it rotates
        internal int Style { get; set; }  // which type of asteroid this is... mostly texture now, but might also be linked to score?

        private const float maxVelocity = 0.2f;  // child volocity = this velocity plus the velocity of the parent
        private const int minSizePercent = 20; // minSizePercent and maxSizePercent are used to scale a child relative to the parent size
        private const int maxSizePercent = 60;
        private const int minAsteroidSize = 20;

        internal Asteroid(Vector2 location) : base (location) {
            this.Status = Status.New;
            this.Orientation = (float)GameModel.Random.Next(0, 100);
            RollRate = GameModel.Random.Next(-300, 300) / 100f;
        }

        internal Asteroid(SerializationInfo info, StreamingContext context) : base(info,context)
        {
            Stage = (int)info.GetValue("Stage", typeof(int));
            RollRate = (float)info.GetValue("RollRate", typeof(float));
            Style = (int)info.GetValue("Style", typeof(int));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Stage", Stage, typeof(int));
            info.AddValue("RollRate", RollRate, typeof(float));
            info.AddValue("Style", Style, typeof(int));
        }

        internal override void Update(GameTime gameTime)
        {
            this.Orientation += (float)(RollRate * Math.PI/18000*gameTime.ElapsedGameTime.TotalMilliseconds);
            base.Update(gameTime);
        }

        // spawns a child asteroid with smaller size, new rotation rate, and slightly different velocity vector
        internal Asteroid spawnChildAsteroid()
        {
            Asteroid newAsteroid = new Asteroid(Location);
            newAsteroid.Velocity = Velocity + randomVelocityVector();

            int newSize = GameModel.Random.Next(minSizePercent, maxSizePercent) * Height / 100;
            if (newSize < minAsteroidSize)
            {
                newSize = minAsteroidSize;
            }
            newAsteroid.Height = newSize;
            newAsteroid.Width = newSize;
            newAsteroid.Stage = Stage - 1;
            newAsteroid.RollRate = GameModel.Random.Next(-(1000 / newSize), (1000 / newSize));
            newAsteroid.Style = Style;

            return newAsteroid;
        }

        internal override void HandleCollision(Entity entityThatCollided)
        {
             if (Status == Status.Active) // ignore dead or new asteroids
            {
                if (Stage > 0)
                {
                    for (int i = 0; i < GameModel.Random.Next(3, 5); i++)
                    {
                        GameModel.AddGameEntity(spawnChildAsteroid());
                    }
                }
                Status = Status.Disposable;
                if (entityThatCollided is Ship)
                {
                    Ship ship = entityThatCollided as Ship;
                    ship.Score += (Stage + 1) * 30;
                }
                else if (entityThatCollided is Laser)
                {
                    Laser laser = entityThatCollided as Laser;
                    GameModel.GetShip(laser.Owner).Score += (Stage+1) * 30;
                }
                
            }
        }

        // create a new velocity with a random direction and speed
        private Vector2 randomVelocityVector()
        {
            float randomX = GameModel.Random.Next((int)(-maxVelocity * 1000),(int)(maxVelocity * 1000)) / 1000f;
            float randomY = GameModel.Random.Next((int)(-maxVelocity * 1000), (int)(maxVelocity * 1000)) / 1000f;
            return new Vector2(randomX, randomY);
        }


    }
}
