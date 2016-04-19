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
    internal class Sprite
    {
        private List<Animation> _animations;    // each sprite contains a list of animations that can be drawn
        private Entity _entity;                 // the entity that this sprite is attached to

        internal Sprite(List<Animation> animations, Entity entity) {
            _animations = animations;
            _entity = entity;
        }

        internal void Update(GameTime gameTime)
        {
            foreach (Animation animation in _animations)
            {
                animation.Update(gameTime);
            }
        }

        internal void Draw(SpriteBatch spriteBatch, Vector2 playerLocation)
        {
            Vector2 position;
            if (!(_entity is Player))
            {
                position = ViewHelper.TranslateModelCoordsToViewCoords(_entity.Location, playerLocation);
            }
            else
            {
                position = Game.ViewCenter;
            }

            // VisualState is an integer with bits set based on the state of the object
            // for example, if the ship is thrusting or shielded or both.
            // This algorithm checks each bit in order from LSB to MSB and draws the corresponding animation
            int entityState = (int)_entity.visualState;
            foreach (Animation animation in _animations)
            {
                if ((entityState & 1) == 1)
                {
                    animation.Draw(spriteBatch, position, _entity.Orientation);
                }
                entityState >>= 1;

                // break early if there aren't any more bits set
                if (entityState == 0)
                {
                    break;
                }
            }

        }

    }
}
