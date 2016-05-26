using EmpireUWP.Network;
using EmpireUWP.View;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EmpireUWP.Model
{
    public abstract class Entity
    {

        private int _entityID = 0;
        internal int EntityID { get { return _entityID; }  } 

        private Vector2 _location = new Vector2(0,0);
        internal Vector2 Location   // location in game coords
        {
            get
            {
                return _location;
            }
            set
            {
                _location.X = value.X;
                _location.Y = value.Y;
            }
        }

        // Height and Width specify the dimensions of the entity
        // Radius needs to be updated whenever they change
        internal float Radius { get; private set; }
        private int _height;
        internal int Height
        {
            get
            {
                return _height;
            }
            set
            {
                _height = value;
                Radius = (_height + _width) / 4;
            }
        }
        private int _width;
        internal int Width
        {
            get
            {
                return _width;
            }
            set
            {
                _width = value;
                Radius = (_height + _width) / 4;
            }
        }

        private Vector2 _velocity;
        internal Vector2 Velocity
        {
            get
            {
                return _velocity;
            }
            set
            {
                _velocity = value;
            }
        }

        internal float Orientation { get; set; }

        internal Status Status { get; set; } // active, dead, new (unitialized), or disposable (can be removed)
        internal VisualStates visualState { get; set; } // used to tell the view what animations to show
        internal EntityType Type { get; set; }

        internal Sprite Renderer { get; set; }

        internal int timeToLive { get; set; }   // object will be removed this many milliseconds after spawning
        internal DateTime LastUpdated { get; private set; }

        protected int age = 0;           // accumulates how much time has passed since the object spawned

        internal Entity()
        {

        }

        internal virtual void Initialize()
        {
            Status = Status.Disposable;
            _entityID = 0;
            visualState = VisualStates.Idle;
        }
   
        internal void GenerateID ()
        {
            _entityID = IDGenerator.NewID();
        }

        internal virtual void SetState(ObjectState info)
        {
            _entityID = info.GetInt("EntityID");
            _location.X = info.GetFloat("LocationX");
            _location.Y = info.GetFloat("LocationY");
            _height = info.GetInt("Height");
            _width = info.GetInt("Width");
            float Radius = info.GetFloat("Radius");
            _velocity.X = info.GetFloat("VelocityX");
            _velocity.Y = info.GetFloat("VelocityY");
            Orientation = info.GetFloat("Orientation");
            visualState = (VisualStates)info.GetInt("VisualState");
            Status = (Status)info.GetInt("Status");
            Type = (EntityType)info.GetInt("Type");
            age = info.GetInt("Age");
        }

        public virtual void GetState(ObjectState info)
        {
            info.AddValue("EntityID", _entityID);
            info.AddValue("LocationX", Location.X);
            info.AddValue("LocationY", Location.Y);
            info.AddValue("Height", Height);
            info.AddValue("Width", Width);
            info.AddValue("Radius", Radius);
            info.AddValue("VelocityX", _velocity.X);
            info.AddValue("VelocityY", _velocity.Y);
            info.AddValue("Orientation", Orientation);
            info.AddValue("VisualState", (int)visualState);
            info.AddValue("Status", (int)Status);
            info.AddValue("Type", (int)Type);
            info.AddValue("Age", age);
        }

        internal void Rotate(float radians)
        {
            Orientation += radians;
        }

        // move the entity in the correct direction, then check  to make sure it hasn't left the
        // play area
        internal void Move(int elapsedTime)
        {
            _location = _location + Velocity * (float)elapsedTime;

            if (_location.X < View.GameView.PlayArea.Left)
            {
                _location.X += View.GameView.PlayArea.Width;
            }
            else if (_location.X > View.GameView.PlayArea.Right)
            {
                _location.X -= View.GameView.PlayArea.Width;
            }

            if (_location.Y < View.GameView.PlayArea.Top)
            {
                _location.Y += View.GameView.PlayArea.Height;
            }
            else if (_location.Y > View.GameView.PlayArea.Bottom)
            {
                _location.Y -= View.GameView.PlayArea.Height;
            }
        }

        internal virtual void Update(int elapsedTime) {
            age += elapsedTime;
            if (timeToLive > 0 && age > timeToLive)
            {
                this.Status = Status.Disposable;
            }

            if(Renderer != null)
            {
                Renderer.Update(elapsedTime);
            }

            Move(elapsedTime);

            LastUpdated = DateTime.Now;  // used to calculate lag updates on packets coming from the network
        }

        internal abstract void HandleCollision(Entity entityThatCollided);

    }
}
