using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using System.Runtime.Serialization;
using EmpireUWP.Network;

namespace EmpireUWP.Model
{
    public class Asteroid : Entity
    {
        public int Stage { get; set; }  // track how many times this rock can split, also influences score value.
        public float RollRate { get; set; }  // how rapidly it rotates
        public int Style { get; set; }  // which type of asteroid this is... mostly texture now, but might also be linked to score?

        private const float maxVelocity = 0.2f;  // child volocity = this velocity plus the velocity of the parent
        private const int minSizePercent = 20; // minSizePercent and maxSizePercent are used to scale a child relative to the parent size
        private const int maxSizePercent = 60;
        private const int minAsteroidSize = 20;

        public Asteroid(GameModel gameModel) : base (gameModel)
        {
 
        }

        public override void Initialize()
        {
            base.Initialize();
            this.visualState = VisualStates.Idle;
            this.Orientation = (float)GameModel.Random.Next(0, 100);
            RollRate = GameModel.Random.Next(-300, 300) / 100f;
        }

        public override void SetState(ObjectState info)
        {
            base.SetState(info);
            Stage = info.GetInt("Stage");
            RollRate = info.GetFloat("RollRate");
            Style = info.GetInt("Style");
        }

        public override void GetState(ObjectState info)
        {
            base.GetState(info);
            info.AddValue("Stage", Stage);
            info.AddValue("RollRate", RollRate);
            info.AddValue("Style", Style);
        }

        public override void Update(int elapsedTime)
        {
            this.Orientation += (float)(RollRate * Math.PI/18000*elapsedTime);
            base.Update(elapsedTime);
        }

        // spawns a child asteroid with smaller size, new rotation rate, and slightly different velocity vector
        public Asteroid spawnChildAsteroid()
        {
            Asteroid newAsteroid = gameModel.worldData.AsteroidFactory();
            newAsteroid.Location = this.Location;
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

        public override void HandleCollision(Entity entityThatCollided)
        {
             if (Status == Status.Active) // ignore dead or new asteroids
            {
                if (Stage > 0)
                {
                    for (int i = 0; i < GameModel.Random.Next(3, 5); i++)
                    {
                        gameModel.AddGameEntity(spawnChildAsteroid());
                    }
                }
                Status = Status.Disposable;
                string owner = "";
                if (entityThatCollided is Ship)
                {
                    Ship ship = entityThatCollided as Ship;
                    owner = ship.Owner;
                }
                else if (entityThatCollided is Laser)
                {
                    Laser laser = entityThatCollided as Laser;
                    owner = laser.Owner;
                }
                //ConnectionManager.IncreaseScore(owner, (Stage + 1) * 30);

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
