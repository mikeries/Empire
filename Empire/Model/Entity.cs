using Empire.View;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Empire.Model
{
    abstract class Entity
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
        // Bounding Circle needs to be updated whenever they change
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
                _boundingCircle.Radius = (_height + _width) / 4;
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
                _boundingCircle.Radius = (_height + _width) / 4;
            }
        }
        
        private Circle _boundingCircle = new Circle(new Vector2(0,0), 0);
        internal Circle BoundingCircle
        {
            get
            {
                return _boundingCircle;
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
  
        internal double Speed {
            get { return Velocity.Length(); }
            set { Velocity = Vector2.Normalize(Velocity) * (float)value; }
        }

        internal float Orientation { get; set; }

        internal Status Status { get; set; } // active, dead, new (unitialized), or disposable (can be removed)
        internal VisualStates visualState { get; set; } // used to tell the view what animations to show
        internal EntityType Type { get; set; }

        internal int timeToLive { get; set; }   // object will be removed this many milliseconds after spawning
        protected int age = 0;           // accumulates how much time has passed since the object spawned

        internal Sprite Renderer;

        internal Entity()
        {
            Initialize();
        }

        internal abstract void Initialize();
   
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
            float radius = info.GetFloat("Radius");
            _boundingCircle = new Circle(Location, radius);
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
            info.AddValue("Radius", BoundingCircle.Radius);
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

            _boundingCircle.Center = _location;
        }

        // accumulate age in milliseconds, and mark the object for removal if beyond its timeToLive
        // then move the object
        internal virtual void Update(int elapsedTime) {
            age += elapsedTime;
            if (timeToLive > 0 && age > timeToLive)
            {
                this.Status = Status.Disposable;
            }
            Move(elapsedTime);
            if(Renderer!=null)
            {
                Renderer.Update(elapsedTime);
            }
        }

        internal virtual void HandleCollision(Entity entityThatCollided) { }

    }
}
