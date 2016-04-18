using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire.View
{
    // Implements a background of space textures with a parallax effect.
    internal static class SpaceBackground
    {
        static List<TiledTexture> textures = new List<TiledTexture>();

        internal static void Initialize() {
            textures.Add(new TiledTexture("Graphics/background1", 0.4f));
            textures.Add(new TiledTexture("Graphics/background2", 0.3f));
            textures.Add(new TiledTexture("Graphics/background3", 0.2f));
        }

        internal static void LoadContent(ContentManager content)
        {
            foreach (TiledTexture texture in textures)
            {
                texture.LoadContent(content);
            }
        }

        internal static void UnloadContent()
        {
            foreach (TiledTexture texture in textures)
            {
                texture.UnloadContent();
            }
        }

        internal static void Draw(SpriteBatch spriteBatch,
            int x,  // the game coordinates of the player's ship (not view coords)
            int y, 
            int screenWidth, 
            int screenHeight)
        {
            foreach (TiledTexture texture in textures)
            {
                // Draw texture using negative of player coords, so that the textures move in the opposite direction
                texture.Draw(spriteBatch, -x, -y, screenWidth, screenHeight);
            }
        }
    }
}
