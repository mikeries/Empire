using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire.Model
{
    class Entity
    {

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
            get { return _velocity; }
            set { _velocity = value; }
        }

        internal double Speed {
            get { return _velocity.Length(); }
            set { _velocity = Vector2.Normalize(_velocity) * (float)value; }
        }

        private float _orientation;
        internal float Orientation
        {
            get { return _orientation; }
            set { _orientation = value; }
        }

        internal Status Status { get; set; } // active, dead, new (unitialized), or disposable (can be removed)
        internal VisualStates visualState { get; set; } // used to tell the view what animations to show
        internal EntityType Type { get; set; }

        internal int timeToLive { get; set; }   // object will be removed this many milliseconds after spawning
        private int _age = 0;           // accumulates how much time has passed since the object spawned

        internal Entity(float x=0, float y=0)
        {
            _location.X = x;
            _location.Y = y;
            Status = Status.New;
            visualState = VisualStates.Idle;

            // TODO:  I'm not sure I like having objects automatically add themselves to the GameEntities list
            // Management of the game entities list is owned by the GameModel class...  this makes sure that 
            // every object gets on the list however, and simplifies things like asteroids creating children when
            // they collide.
            GameModel.GameEntities.Add(this);
        }

        internal void Rotate(float radians)
        {
            _orientation += radians;
        }

        // move the entity in the correct direction, then check  to make sure it hasn't left the
        // play area
        internal void Move(GameTime gameTime)
        {
            _location = _location + _velocity * (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_location.X < View.Game.PlayArea.Left)
            {
                _location.X += View.Game.PlayArea.Width;
            }
            else if (_location.X > View.Game.PlayArea.Right)
            {
                _location.X -= View.Game.PlayArea.Width;
            }

            if (_location.Y < View.Game.PlayArea.Top)
            {
                _location.Y += View.Game.PlayArea.Height;
            }
            else if (_location.Y > View.Game.PlayArea.Bottom)
            {
                _location.Y -= View.Game.PlayArea.Height;
            }

            _boundingCircle.Center = _location;
        }

        // accumulate age in milliseconds, and mark the object for removal if beyond its timeToLive
        // then move the object
        internal virtual void Update(GameTime gameTime) {
            _age += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (timeToLive > 0 && _age > timeToLive)
            {
                this.Status = Status.Dead;
            }
            Move(gameTime);
        }

        internal virtual void HandleCollision(Entity entityThatCollided) { }
    }
}
