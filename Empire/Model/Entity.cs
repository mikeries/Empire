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

        public Vector2 Location   // location in game coords
        {
            get { return new Vector2(_x, _y); }
            internal set { _x = value.X; _y = value.Y; }
        }
        protected float _x;
        public float X { get { return _x; } }
        protected float _y;
        public float Y { get { return _y; } }

        public Rectangle BoundingBox { get { return new Rectangle((int)_x-Width/2, (int)_y - Height/2, Width, Height); } }
        public int Height { get; set; }
        public int Width { get; set; }

        public Circle BoundingCircle { get { return new Circle(Location, (Width+Height)/4);} }

        internal Vector2 _velocity;
        internal Vector2 Velocity { get { return _velocity; } }
        internal double Speed {
            get { return _velocity.Length(); }
            set { _velocity = Vector2.Normalize(_velocity) * (float)value; }
        }

        private float _orientation;
        public float Orientation
        {
            get { return _orientation; }
            internal set { _orientation = value; }
        }

        public Status Status { get; set; } // active, dead, new (unitialized), or disposable (can be removed)
        public int visualState { get; set; } // used to tell the view what animations to show

        public int timeToLive { get; set; }
        private int _elapsedTime=0;

        public EntityType Type { get; internal set; }

        internal Entity(int x=0, int y=0)
        {
            _x = x;
            _y = y;
            Status = Status.New;
            visualState = 1;
        }

        internal void setVelocity(Vector2 velocity)
        {
            _velocity = velocity;
        }

        internal void Rotate(float radians)
        {
            // _velocity = Vector2.Transform(_velocity, Matrix.CreateRotationZ(radians));
            _orientation += radians;
        }

        public void Move(GameTime gameTime)
        {
            _x = (float)(_x+ _velocity.X * gameTime.ElapsedGameTime.TotalMilliseconds / 10);
            if (_x < View.Game.PlayArea.Left) _x += View.Game.PlayArea.Width;
            if (_x > View.Game.PlayArea.Right) _x -= View.Game.PlayArea.Width;
            _y = (float)(_y+_velocity.Y * gameTime.ElapsedGameTime.TotalMilliseconds / 10);
            if (_y < View.Game.PlayArea.Top) _y += View.Game.PlayArea.Height;
            if (_y > View.Game.PlayArea.Bottom) _y -= View.Game.PlayArea.Height;

        }

        public virtual void Update(GameTime gameTime) {
            _elapsedTime += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (timeToLive > 0 && _elapsedTime > timeToLive)
            {
                this.Status = Status.Dead;
            }
            Move(gameTime);
        }
    }
}
