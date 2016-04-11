using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire.View
{
    // Implements a background of space with a parallax effect.
    class SpaceBackground
    {
        List<TiledTexture> textures = new List<TiledTexture>();

        public SpaceBackground() { }

        //TODO: might be a better idea to pull speed factor out of the TiledTexture class and put it in this class,
        // calling the texture with an offset
        public void Initialize() {
            textures.Add(new TiledTexture("Graphics/background1", 0.4f));
            textures.Add(new TiledTexture("Graphics/background2", 0.3f));
            textures.Add(new TiledTexture("Graphics/background3", 0.2f));
        }

        public void LoadContent(ContentManager content)
        {
            foreach (TiledTexture texture in textures)
                texture.LoadContent(content);
        }

        public void UnloadContent()
        {
            foreach (TiledTexture texture in textures)
                texture.UnloadContent();
        }

        public void Draw(SpriteBatch spriteBatch, int x, int y, int screenWidth, int screenHeight) {
            foreach (TiledTexture texture in textures)
                texture.Draw(spriteBatch,-x, -y, screenWidth, screenHeight); // use negative of player location so that the textures move the opposite way.
        }
    }
}
