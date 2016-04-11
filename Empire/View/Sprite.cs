using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Empire.Model;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Empire.View
{
    class Sprite
    {
        private List<Animation> _animations;
        private Entity _entity;

        public Sprite(List<Animation> animations, Entity entity) {
            _animations = animations;
            _entity = entity;
        }

        public void Update(GameTime gameTime)
        {
            foreach (Animation animation in _animations)
                animation.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 playerLocation)
        {
            Vector2 position;
            if (!(_entity is Player))
                position = ViewHelper.translateModelCoordsToViewCoords((int)_entity.X,(int) _entity.Y, playerLocation);
            else
                position = Game.ViewCenter;

            int entityState = _entity.visualState;
            foreach (Animation animation in _animations)
            {
                if ((entityState & 1) == 1)
                   animation.Draw(spriteBatch, position, _entity.Orientation);
                entityState >>= 1;
            }

        }

    }
}
