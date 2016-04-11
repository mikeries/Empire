using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Empire.Model;
using System.Linq;
using System;

namespace Empire.View
{
    static class ViewHelper
    {
        public static Dictionary<string, Animation> _templates = new Dictionary<string, Animation>();
        private static Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();
        private static Dictionary<string, SpriteFont> _fonts = new Dictionary<string, SpriteFont>();

        public static List<Animation> AnimationFactory(Entity entity) {
            List<Animation> animationCollection = new List<Animation>();
            Animation animation;

            // Note:  Animations must be added to the collection in the right order, as they will be drawn in the order added.
            if (entity is Player)
            {
                animation = _templates["player"].Clone();
                animation.Zorder = 0.4f; // top... TODO: create enum for the various layers?
                animationCollection.Add(animation);

                animation = _templates["shipThrusting"].Clone();
                animation.Zorder = 0.4f;
                animationCollection.Add(animation);

                animation = _templates["shieldStrip"].Clone();
                animation.Zorder = 0.4f;
                animationCollection.Add(animation);

                return animationCollection;
            }
            if (entity is Planet) {
                Planet planet = entity as Planet;

                animation = _templates["planet"].Clone();
                animation.SpriteStrip = _textures[planet.planetID.ToString()];
                animation.scale = (float)planet.Height / 245;  // 245 is the height/width of the planet texture.
                animation.Zorder = 0.1f;
                animationCollection.Add(animation);
                return animationCollection;
            }

            if (entity is Asteroid)
            {
                Asteroid asteroid = entity as Asteroid;
                animation = _templates["asteroid" + asteroid.Style].Clone();
                animation.scale = (float)entity.Height / 150;  // ~150 is the height/width of the asteroid texture.
                animation.Zorder = 0.3f;
                animationCollection.Add(animation);
                return animationCollection;
            }

            if (entity is Laser)
            {
                animation = _templates["laser"].Clone();
                animation.Zorder = 0.4f;
                animationCollection.Add(animation);
                return animationCollection;
            }

            return null;
        }

        public static Vector2 translateModelCoordsToViewCoords(int x, int y, Vector2 playerLocation)
        {
            return new Vector2(x - playerLocation.X + 640, y  - playerLocation.Y + 360);
        }

        // load the textures and create the animation templates
        // TODO:  Add error handling (and eventually read from config files)
        public static void LoadContent(ContentManager content)
        {
            _textures.Add("texture1x1", content.Load<Texture2D>("Graphics/texture1x1"));  // simple 1x1 texture used for line drawing
            _textures.Add("ship", content.Load<Texture2D>("Graphics/ship"));
            _textures.Add("shipThrusting", content.Load<Texture2D>("Graphics/shipStrip"));
            _textures.Add("shieldStrip", content.Load<Texture2D>("Graphics/shieldStrip"));
            _textures.Add("planet1", content.Load<Texture2D>("Graphics/planet1"));
            _textures.Add("planet2", content.Load<Texture2D>("Graphics/planet2"));
            _textures.Add("planet3", content.Load<Texture2D>("Graphics/planet3"));
            _textures.Add("planet4", content.Load<Texture2D>("Graphics/planet4"));
            _textures.Add("planet5", content.Load<Texture2D>("Graphics/planet5"));
            _textures.Add("planet6", content.Load<Texture2D>("Graphics/planet6"));
            _textures.Add("planet7", content.Load<Texture2D>("Graphics/planet7"));
            _textures.Add("planet8", content.Load<Texture2D>("Graphics/planet8"));
            _textures.Add("planet9", content.Load<Texture2D>("Graphics/planet9"));
            _textures.Add("planet10", content.Load<Texture2D>("Graphics/planet10"));
            _textures.Add("planet11", content.Load<Texture2D>("Graphics/planet11"));
            _textures.Add("planet12", content.Load<Texture2D>("Graphics/planet12"));
            _textures.Add("planet13", content.Load<Texture2D>("Graphics/planet13"));
            _textures.Add("planet14", content.Load<Texture2D>("Graphics/planet14"));
            _textures.Add("planet15", content.Load<Texture2D>("Graphics/planet15"));
            _textures.Add("planet16", content.Load<Texture2D>("Graphics/planet16"));
            _textures.Add("asteroid1", content.Load<Texture2D>("Graphics/asteroid"));
            _textures.Add("asteroid2", content.Load<Texture2D>("Graphics/asteroid2"));
            _textures.Add("asteroid3", content.Load<Texture2D>("Graphics/asteroid3"));
            _textures.Add("laser", content.Load<Texture2D>("Graphics/playerbullet"));

            _fonts.Add("Score24", content.Load<SpriteFont>("Score24"));
            _fonts.Add("Arial12", content.Load<SpriteFont>("Arial12"));

            _templates.Add("player",new Animation(_textures["ship"], 64, 64, 1, 50, true));
            _templates.Add("shipThrusting", new Animation(_textures["shipThrusting"], 64, 64, 4, 50, true));
            _templates.Add("shieldStrip", new Animation(_textures["shieldStrip"], 87, 87, 4, 50, true));
            _templates.Add("planet", new Animation(_textures["planet1"], 
                _textures["planet1"].Width, _textures["planet1"].Height, 1, 1000, true));
            _templates.Add("asteroid1", new Animation(_textures["asteroid1"], _textures["asteroid1"].Width, _textures["asteroid1"].Height, 1, 100, true));
            _templates.Add("asteroid2", new Animation(_textures["asteroid2"], _textures["asteroid2"].Width, _textures["asteroid2"].Height, 1, 100, true));
            _templates.Add("asteroid3", new Animation(_textures["asteroid3"], _textures["asteroid3"].Width, _textures["asteroid3"].Height, 1, 100, true));
            _templates.Add("laser", new Animation(_textures["laser"], _textures["laser"].Width, _textures["laser"].Height, 1, 100, true));
        }

        public static void UnloadContent()
        {
            foreach (string textureName in _textures.Keys.ToList())
            {
                _textures[textureName].Dispose();
                _textures.Remove(textureName);
            }
        }

        public static void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end)
        {
            Vector2 line = end - start;
            float angleToRotate =(float)Math.Atan2(line.Y, line.X);

            spriteBatch.Draw(_textures["texture1x1"],
                new Rectangle((int)start.X,(int)start.Y, (int)line.Length(), 1), 
                null, Color.White, angleToRotate, new Vector2(0, 0), SpriteEffects.None, 0);

        }

        public static void DrawRectangle(SpriteBatch spriteBatch,Rectangle rect, Color color)
        {

            spriteBatch.Draw(_textures["texture1x1"], rect, null, color, 0, new Vector2(0, 0), SpriteEffects.None, 0);
        }

        // TODO: add error checking.  Maybe return a default font?
        public static SpriteFont GetFont(string font)
        {
            return _fonts[font];
        }
    }
}
