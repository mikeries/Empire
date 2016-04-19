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
        private const double MaxSpeed = 4;
        private const float engineThrust = 0.01f;
        private const int timeBetweenShots = 40;
        private const float RotationRate = (float)Math.PI / 1000f;
        private const float Stopped = 0.0001f;

        internal int ShieldEnergy { get; set; }

        internal Player(float x, float y) : base(x,y)
        {
            Velocity = new Vector2(Stopped, 0);
            this.Type = EntityType.Ship;
            ShieldEnergy = 100;
        }

        internal override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        internal void Initialize()
        {
            Location = new Vector2(0, 0);
            Velocity = new Vector2(Stopped, 0);
        }

        public void RotateShip(Direction direction,int elapsedTime) {
            switch (direction)
            {
                case Direction.Left:
                    Rotate(-1* RotationRate * elapsedTime);
                    break;
                case Direction.Right:
                    Rotate(RotationRate * elapsedTime);
                    break;
                default: break;
            }
        }

        public void AccelerateShip(Direction direction, int elapsedTime)
        {
            switch (direction)
            {
                case Direction.Up:
                    visualState |= VisualStates.Thrusting;
                    Vector2 acceleration = ModelHelper.thrustVector(engineThrust*elapsedTime, Orientation);
                    Velocity = Velocity + acceleration;
                    if (Speed > MaxSpeed)
                    {
                        Speed = MaxSpeed;
                    }
                    break;
                case Direction.Down:
                    if (ShieldEnergy > 0)
                    {
                        visualState |= VisualStates.Shields;
                    }
                    break;
                default:
                    //TODO: Log this... it shouldn't happen
                    break;
            }
        }

        public Entity Fire(GameTime gameTime)
        {
            Laser laser = ModelHelper.LaserFactory(this);
            laser.Update(gameTime);
            return laser;
        }

    }

}
