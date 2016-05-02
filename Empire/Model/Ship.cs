using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Empire.Network;
using System.Runtime.Serialization;

namespace Empire.Model
{

    class Ship : Entity
    {
        internal ShipCommand Command = new ShipCommand("",0);        // container for commands issued by player, network, AI

        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const double MaxSpeed = .8;                  // max velocity
        private const float engineThrust = 0.001f;           // factor controlling how rapidly velocity increases
        private const float Stopped = 0.0001f;              // velocity when stopped.  We can't use 0 because we need to preserve
                                                            // the direction of the vector

        private const int millisecondsPerShot = 200;         // minimum number of ms between shots
        private static int _timeSinceLastShot;              // accumulated time since the weapon last fired

        private const float RotationRate = (float)Math.PI / 1500f;
        private const int millisecondsPerRotation = 10;     // minimum number of ms between rotations

        private const float ShieldDecayRate = 0.15f;        // shield decay rate per millisecond
        private const float ShieldRegenerationRate = 0.1f;  // shield regen rate per millisecond

        private const int ShipHeight = 64;                  // nominal ship height;
        private const int ShipWidth = 64;                   // nominal ship width;

        internal int Score;                                 // Number of points this ship has scored

        internal int ShieldEnergy { get; set; }

        internal string Owner { get; set; }     

        internal Ship() : base()
        {
            Initialize();
        }

        internal override void Initialize()
        {
            Location = new Vector2(0, 0);
            Velocity = new Vector2(Stopped, 0);
            this.Type = EntityType.Ship;
            Height = ShipHeight;
            Width = ShipWidth;
            ShieldEnergy = 100;
        }

        internal override void SetState(ObjectState info)
        {
            base.SetState(info);
            //_timeSinceLastShot = (int)info.GetValue("timeSinceLastShot", typeof(int));
            Score = info.GetInt("Score");
            ShieldEnergy = info.GetInt("ShieldEnergy");
            Owner = info.GetString("Owner");
        }

        public override void GetState(ObjectState info)
        {
            base.GetState(info);
            //info.AddValue("timeSinceLastShot", _timeSinceLastShot);
            info.AddValue("Score", Score);
            info.AddValue("ShieldEnergy", ShieldEnergy);
            info.AddValue("Owner", Owner);
        }

        internal override void Update(int elapsedTime)
        {

            if (ConnectionManager.IsHost)
            {
                ProcessInput(elapsedTime);
            }

            ShieldEnergy += (int)(elapsedTime * ShieldRegenerationRate);
            ShieldEnergy = MathHelper.Clamp(ShieldEnergy, 0, 100);

            base.Update(elapsedTime);
        }


        private void ProcessInput(int elapsedTime)
        {

            visualState = VisualStates.Idle; // reset visual state

            if (Command.Left)
            {
                RotateShip(Direction.Left, elapsedTime);
            }
            if (Command.Right)
            {
                RotateShip(Direction.Right, elapsedTime);
            }
            if (Command.Thrust)
            {
                AccelerateShip(elapsedTime);
            }
            if (Command.Shields)
            {
                RaiseShields(elapsedTime);
            }

            _timeSinceLastShot += elapsedTime;
            if (Command.Shoot)
            {
                if (_timeSinceLastShot > millisecondsPerShot)
                {
                    Fire(elapsedTime);
                    _timeSinceLastShot = 0;
                }
            }

        }

        internal void RaiseShields(int elapsedTime)
        {
            ShieldEnergy -= (int)(elapsedTime * ShieldDecayRate);
            if (ShieldEnergy > 0)
            {
                visualState |= VisualStates.Shields;
            }
        }

        internal void RotateShip(Direction direction,int elapsedTime) {
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

        internal void AccelerateShip(int elapsedTime)
        {
            visualState |= VisualStates.Thrusting;
            Vector2 acceleration = ModelHelper.thrustVector(engineThrust*elapsedTime, Orientation);
            Velocity = Velocity + acceleration;
            if (Speed > MaxSpeed)
            {
                Speed = MaxSpeed;
            }
        }

        internal void Fire(int elapsedTime)
        {
            Laser laser = ModelHelper.LaserFactory(this);
             laser.Update(elapsedTime);
            GameModel.AddGameEntity(laser);
        }

        internal override void HandleCollision(Entity entityThatCollided)
        {
            if(ShieldsAreDown())
            {
                Score -= 100;  
            }
        }

        private bool ShieldsAreDown()
        {
            return ((int)visualState & (int)VisualStates.Shields) == 0;
        }

    }

}
