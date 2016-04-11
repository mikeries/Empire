using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire.Model
{

    class Player : Entity
    {
        private const double MaxSpeed = 5;
        private static float engineThrust = 0.01f;
        private const int timeBetweenShots = 40;
        internal int shieldEnergy = 100;
        
        public Player(int x, int y) : base(x,y)
        {
            _velocity = new Vector2(0.0001f, 0);
            this.Type = EntityType.Ship;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public void Initialize()
        {
            _x = 0;
            _y = 0;
            _velocity = new Vector2(0.0001f, 0);
        }

        // TODO: need to scale by gameTime
        public void RotateShip(Direction direction,int elapsedTime) {
            switch (direction)
            {
                case Direction.Left: Rotate((float)Math.PI/1000*-1*elapsedTime); break;
                case Direction.Right: Rotate((float)Math.PI/1000*elapsedTime); break;
                default: break;
            }
        }

        public void AccelerateShip(Direction direction, int elapsedTime)
        {
            switch (direction)
            {
                case Direction.Up:
                    visualState |= (int)VisualStates.Thrusting;
                    Vector2 acceleration = thrustVector(engineThrust*elapsedTime, Orientation);
                    _velocity = _velocity + acceleration;
                    if (Speed > MaxSpeed) Speed = MaxSpeed;
                    break;
                case Direction.Down:
                    if (shieldEnergy > 0)
                        visualState |= (int)VisualStates.Shields;
                   // Speed = 0.00001f;
                    break;
                default: break;
            }
        }

        // TODO: need to move the details of laser spawning to ModelHelper
        public Entity Fire(GameTime gameTime)
        {
            Laser laser = new Laser();
            laser.Location = new Vector2(this.X, this.Y);
            laser.Height = 3;
            laser.Width = 3;
            laser.Orientation = this.Orientation;
            laser.setVelocity(_velocity + thrustVector(10,Orientation));
            laser.Update(gameTime);
            return laser;
        }

        private static Vector2 thrustVector(float thrust, float orientation)
        {
            return new Vector2(thrust * (float)Math.Sin(orientation), -thrust * (float)Math.Cos(orientation));
        }

    }

}
