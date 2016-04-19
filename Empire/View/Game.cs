using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Empire.Model;
using System.Linq;
using System;

// TODO: Move the spites into a class of their own for the same reason

namespace Empire.View
{
    public class Game : Microsoft.Xna.Framework.Game
    {
        private static readonly log4net.ILog log = 
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        internal static Rectangle PlayArea = new Rectangle(0, 0, 40000, 40000);
        internal static Vector2 WindowSize = new Vector2(1280f, 760f);
        internal static Vector2 ViewCenter = new Vector2(WindowSize.X/2, WindowSize.Y/2);
        internal static int GameTime = 1000 * 60 * 2;  // 2 minute timer in milliseconds

        private bool _gameOver { get; set; }
        public bool GameOver { get; set; }

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private GraphicalUserInterface _gui;
        
        private Model.GameModel _model = new Model.GameModel();
        private Model.Player _player;

        private Dictionary<Entity, Sprite> _sprites = new Dictionary<Entity, Sprite>();
        private Dictionary<string, Animation> _animations = new Dictionary<string, Animation>();

        internal Game()
        {
            _graphics = new GraphicsDeviceManager(this);
            _gui = new GraphicalUserInterface(this, _model);

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            // set up display size
            _graphics.PreferredBackBufferHeight = (int)WindowSize.Y;
            _graphics.PreferredBackBufferWidth = (int)WindowSize.X;
            _graphics.ApplyChanges();

            _spriteBatch = new SpriteBatch(GraphicsDevice);

            SpaceBackground.Initialize();
            _model.Initialize();
            _gui.Initialize();

            _gameOver = false;
            _player = _model.Player;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            SpaceBackground.LoadContent(Content);
            ViewHelper.LoadContent(Content);
            _gui.LoadContent(Content);
        }

        protected override void UnloadContent()
        {
            SpaceBackground.UnloadContent();
            ViewHelper.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _model.Update(gameTime);
            _player.Update(gameTime);
            updateSprites(gameTime);
            _gui.Update(gameTime);

            if (GameOver)
            {
                EndGame();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // Spacebackground first
            _spriteBatch.Begin();
            SpaceBackground.Draw(_spriteBatch,
                (int)_player.Location.X,
                (int)_player.Location.Y,
                GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height
            );
            _spriteBatch.End();


            // Use front-to-back sort mode so that the zOrder works correctly.  
            // An alternative choice would be to plot each entity type in a separate spriteBatch.
            _spriteBatch.Begin(SpriteSortMode.FrontToBack);
            foreach (Sprite sprite in _sprites.Values)
            {
                sprite.Draw(_spriteBatch, _player.Location);
            }
            _spriteBatch.End();

            // use a new Begin() to draw the GUI elements so that they always appear on top
            // of everything else
            _spriteBatch.Begin();
            _gui.Draw(_spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void updateSprites(GameTime gameTime)
        {
            // remove dead entities
            var deadEntities =
                from entity in _model.GameEntities
                where entity.Status == Status.Dead
                select entity;
            foreach (Entity entity in deadEntities)
            {
                _sprites.Remove(entity);
                entity.Status = Status.Disposable;
            }

            // add new ones
            var newEntities =
                from entity in _model.GameEntities
                where entity.Status == Status.New
                select entity;
            foreach (Entity entity in newEntities)
            {
                List<Animation> animationCollection = ViewHelper.AnimationFactory(entity);
                Sprite newSprite = new Sprite(animationCollection, entity);
                _sprites.Add(entity, newSprite);
                entity.Status = Status.Active;
            }

            // update all
            foreach (Sprite sprite in _sprites.Values)
            {
                sprite.Update(gameTime);
            }
        }

        internal void EndGame()
        {
            _gameOver = true;
            
            //TODO: pause here for a key and add NewGame()
            Console.ReadLine();
        }
    }
}
