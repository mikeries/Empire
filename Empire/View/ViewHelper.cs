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
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static Dictionary<string, Animation> _templates = new Dictionary<string, Animation>();
        private static Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();
        private static Dictionary<string, SpriteFont> _fonts = new Dictionary<string, SpriteFont>();

        //TODO: read animations from config file
        internal static List<Animation> AnimationFactory(Entity entity)
        {
            List<Animation> animationCollection = new List<Animation>();
            Animation animation;

            // Note:  Animations must be added to the collection in the right order, as they will be drawn in the order added.
            if (entity is Ship)
            {
                animation = _templates["player"].Clone();
                animationCollection.Add(animation);

                animation = _templates["shipThrusting"].Clone();
                animationCollection.Add(animation);

                animation = _templates["shieldStrip"].Clone();
                animationCollection.Add(animation);

                return animationCollection;
            }
            if (entity is Planet)
            {
                Planet planet = entity as Planet;

                animation = _templates["planet"].Clone();
                animation.Scale = (float)planet.Height / _textures["planet1"].Height;
                //TODO: need to allow the texture to be set based on the planet ID.
                animationCollection.Add(animation);
                return animationCollection;
            }

            if (entity is Asteroid)
            {
                Asteroid asteroid = entity as Asteroid;
                animation = _templates["asteroid" + asteroid.Style].Clone();
                animation.Scale = (float)entity.Height / _textures["asteroid" + asteroid.Style].Height;
                animationCollection.Add(animation);
                return animationCollection;
            }

            if (entity is Laser)
            {
                animation = _templates["laser"].Clone();
                animationCollection.Add(animation);
                return animationCollection;
            }

            // if we reach here, it is an error, so log it and throw an exception
            log.Error("Unable to create sprite -- unknown entity in the collection.");
            return null;
        }

        // determine view coordinates based on the model coords of the entity relative to the player
        internal static Vector2 TranslateModelCoordsToViewCoords(Vector2 entityLocation, Vector2 playerLocation)
        {
            // return new Vector2(x - playerLocation.X + Game.ViewCenter.X, y - playerLocation.Y + Game.ViewCenter.Y);
            return entityLocation - playerLocation + Game.ViewCenter;
        }

        // load the textures and create the animation templates
        // TODO: Add error handling
        // TODO: read data from config files
        internal static void LoadContent(ContentManager content)
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

            _templates.Add("player", new Animation(_textures["ship"], 64, 64, 1, 50, true, CanvasLayer.Ship));
            _templates.Add("shipThrusting", new Animation(_textures["shipThrusting"], 64, 64, 4, 50, true, CanvasLayer.Ship));
            _templates.Add("shieldStrip", new Animation(_textures["shieldStrip"], 87, 87, 4, 50, true, CanvasLayer.Ship));
            _templates.Add("planet", new Animation(_textures["planet1"],
                 _textures["planet1"].Width, _textures["planet1"].Height, 1, 1000, true, CanvasLayer.Planet));
            _templates.Add("asteroid1", new Animation(_textures["asteroid1"], _textures["asteroid1"].Width, _textures["asteroid1"].Height, 1, 100, true, CanvasLayer.Asteroid));
            _templates.Add("asteroid2", new Animation(_textures["asteroid2"], _textures["asteroid2"].Width, _textures["asteroid2"].Height, 1, 100, true, CanvasLayer.Asteroid));
            _templates.Add("asteroid3", new Animation(_textures["asteroid3"], _textures["asteroid3"].Width, _textures["asteroid3"].Height, 1, 100, true, CanvasLayer.Asteroid));
            _templates.Add("laser", new Animation(_textures["laser"], _textures["laser"].Width, _textures["laser"].Height, 1, 100, true, CanvasLayer.Laser));

            log.Info("Textures loaded.");
        }

        internal static void UnloadContent()
        {
            foreach (string textureName in _textures.Keys.ToList())
            {
                _textures[textureName].Dispose();
                _textures.Remove(textureName);
            }
        }

        // currently not used
        //private static void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end)
        //{
        //    Vector2 line = end - start;
        //    float angleToRotate =(float)Math.Atan2(line.Y, line.X);

        //    spriteBatch.Draw(_textures["texture1x1"],
        //        new Rectangle((int)start.X,(int)start.Y, (int)line.Length(), 1), 
        //        null, 
        //        Color.White, 
        //        angleToRotate, 
        //        new Vector2(0, 0),  // rotation origin
        //        SpriteEffects.None, 
        //        0
        //    );
        //}

        // DrawRectangle method stretches a 1x1 texture into the specified length and width
        // used for StatusBars
        internal static void DrawRectangle(SpriteBatch spriteBatch, Rectangle rect, Color color)
        {
            spriteBatch.Draw(_textures["texture1x1"],
                rect,
                null,
                color,
                0,
                new Vector2(0, 0),
                SpriteEffects.None,
                0
            );
        }

        // TODO: add error checking.  Maybe return a default font?
        internal static SpriteFont GetFont(string font)
        {
            return _fonts[font];
        }
    }
}
