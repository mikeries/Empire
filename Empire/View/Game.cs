using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Empire.Model;
using System.Linq;
using System;

// TODO: Move the spites into a class of their own

namespace Empire.View
{
    public class Game : Microsoft.Xna.Framework.Game
    {
        private static readonly log4net.ILog log = 
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        internal static Rectangle PlayArea = new Rectangle(0, 0, 20000, 20000);
        internal static Vector2 WindowSize = new Vector2(1280f, 760f);
        internal static Vector2 ViewCenter = new Vector2(WindowSize.X/2, WindowSize.Y/2);
        internal static int GameDuration = 1000 * 60 * 2;  // 2 minute timer in milliseconds

        internal bool GameOver { get; set; }
        internal int Score { get { return _player.Score; } }
        internal int ShieldEnergy { get { return _player.ShieldEnergy; } }
        internal int TimeRemaining { get; set; }

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private GraphicalUserInterface _gui;
        private AIComponent _ai = new AIComponent();

        private int _shipIndex = 0;
        private Model.Ship _player;
        internal Model.Ship Player { get { return _player; } }

        private Dictionary<Entity, Sprite> _sprites = new Dictionary<Entity, Sprite>();
        private Dictionary<string, Animation> _animations = new Dictionary<string, Animation>();

        internal Game()
        {
            _graphics = new GraphicsDeviceManager(this);
            _gui = new GraphicalUserInterface(this);

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            // set up display size
            _graphics.PreferredBackBufferHeight = (int)WindowSize.Y;
            _graphics.PreferredBackBufferWidth = (int)WindowSize.X;
            _graphics.ApplyChanges();

            _spriteBatch = new SpriteBatch(GraphicsDevice);

            TimeRemaining = GameDuration;

            SpaceBackground.Initialize();
            GameModel.Initialize();

            _shipIndex = 0;
            _player = GameModel.Ships[_shipIndex];

            _gui.Initialize();

            GameOver = false;

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

            // testing code to switch player control to other ships
            if(Keyboard.GetState().IsKeyDown(Keys.Add))
            {
                _shipIndex++;
                if(_shipIndex == GameModel.Ships.Count)
                {
                    _shipIndex = 0;
                }
                _player = GameModel.Ships[_shipIndex];
            }

            ProcessLocalInput();

            _ai.Update(_player);

            GameModel.Update(gameTime);
            updateSprites(gameTime);
            _gui.Update(gameTime);

            TimeRemaining -= (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (GameOver)
            {
                EndGame();
            }

            base.Update(gameTime);
        }

        // process input and handle it, or dispatch it to the GameModel to be processed
        internal void ProcessLocalInput()
        {
            KeyboardState keyboardState = Keyboard.GetState();

            // eventually, there will be commands to bring up the local minimap, or zoom in/out, chat, etc, but
            // for now, we only have commands to be passed to the ship.

            // In the future, dispatching commands to the world will be done through the network interface
            // currently, we'll just send the command directly.

            _player.Command = new ShipCommand(keyboardState);

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

            // game entities next
            // Use front-to-back sort mode so that the zOrder works correctly.  
            // An alternative choice would be to plot each entity type in a separate spriteBatch.
            //_spriteBatch.Begin(SpriteSortMode.FrontToBack);
            // Note: front-to-back causes asteroids to randomly shimmer infront/behind each other because they are all at the
            // same depth.  So let them sort themselves out...
            _spriteBatch.Begin();
            foreach (Sprite sprite in _sprites.Values)
            {
                sprite.Draw(_spriteBatch, _player);
            }
            _spriteBatch.End();

            // Now the GUI elements
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
                from entity in GameModel.GameEntities
                where entity.Status == Status.Dead
                select entity;
            foreach (Entity entity in deadEntities)
            {
                _sprites.Remove(entity);
                entity.Status = Status.Disposable;
            }

            // add new ones
            var newEntities =
                from entity in GameModel.GameEntities
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
            GameOver = true;
            
            //TODO: pause here for a key and add NewGame()
            Console.ReadLine();
        }
    }
}
