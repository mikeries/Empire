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
    [Serializable]
    class Entity : ISerializable
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
        private int _age = 0;           // accumulates how much time has passed since the object spawned

        internal Sprite Renderer;

        internal Entity(Vector2 location)
        {
            Location = location;
            Status = Status.New;
            visualState = VisualStates.Idle;
        }

        internal void GenerateID ()
        {
            _entityID = IDGenerator.NewID();
        }

        internal Entity(SerializationInfo info, StreamingContext context)
        {
            _entityID = (int)info.GetValue("EntityID", typeof(int));
            _location.X = (float)info.GetValue("LocationX", typeof(float));
            _location.Y = (float)info.GetValue("LocationY", typeof(float));
            _height = (int)info.GetValue("Height", typeof(int));
            _width = (int)info.GetValue("Width", typeof(int));
            float radius = (float)info.GetValue("Radius", typeof(float));
            _boundingCircle = new Circle(Location, radius);
            _velocity.X = (float)info.GetValue("VelocityX", typeof(float));
            _velocity.Y = (float)info.GetValue("VelocityY", typeof(float));
            Orientation = (float)info.GetValue("Orientation", typeof(float));
            visualState = (VisualStates)info.GetValue("VisualState", typeof(int));
            Status = (Status)info.GetValue("Status", typeof(int));
            Type = (EntityType)info.GetValue("Type", typeof(int));
            _age = (int)info.GetValue("Age", typeof(int));
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("EntityID", _entityID, typeof(int));
            info.AddValue("LocationX", Location.X, typeof(float));
            info.AddValue("LocationY", Location.Y, typeof(float));
            info.AddValue("Height", Height, typeof(int));
            info.AddValue("Width", Width, typeof(int));
            info.AddValue("Radius", BoundingCircle.Radius, typeof(float));
            info.AddValue("VelocityX", _velocity.X,typeof(float));
            info.AddValue("VelocityY", _velocity.Y, typeof(float));
            info.AddValue("Orientation", Orientation, typeof(float));
            info.AddValue("VisualState", (int)visualState, typeof(int));
            info.AddValue("Status", (int)Status, typeof(int));
            info.AddValue("Type", (int)Type, typeof(int));
            info.AddValue("Age", _age, typeof(int));
        }

        internal void Rotate(float radians)
        {
            Orientation += radians;
        }

        // move the entity in the correct direction, then check  to make sure it hasn't left the
        // play area
        internal void Move(GameTime gameTime)
        {
            _location = _location + Velocity * (float)gameTime.ElapsedGameTime.TotalMilliseconds;

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
        internal virtual void Update(GameTime gameTime) {
            _age += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (timeToLive > 0 && _age > timeToLive)
            {
                this.Status = Status.Disposable;
            }
            Move(gameTime);
            if(Renderer!=null)
            {
                Renderer.Update(gameTime);
            }
        }

        internal virtual void HandleCollision(Entity entityThatCollided) { }

    }
}
