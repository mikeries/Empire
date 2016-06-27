using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using EmpireUWP.Model;
using EmpireUWP.Network;
using System.Linq;
using System;

namespace EmpireUWP.View
{
    public class GameView : Microsoft.Xna.Framework.Game
    {
        internal static Rectangle PlayArea = new Rectangle(0, 0, 20000, 20000);
        internal static Vector2 WindowSize = new Vector2(1280f, 720f);
        internal static Vector2 ViewCenter = new Vector2(WindowSize.X/2, WindowSize.Y/2);
        internal static int GameDuration = 1000 * 60 * 2;  // 2 minute timer in milliseconds

        internal bool GameOver { get; set; }

        //TODO: fix scoring system
        internal int Score
        {
            get
            {
                if (_player.Owner != null)
                {
                    return 0;                    
                }
                else
                {
                    return 0;
                }
            }
        }

        internal int ShieldEnergy
        {
            get
            {
                if (_player.Owner != null)
                {
                    return _player.ShieldEnergy;
                }
                else
                {
                    return 0;
                }
            }
        }

        internal int TimeRemaining { get; set; }

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private GraphicalUserInterface _gui;
        private AIComponent _ai = new AIComponent();
        private ConnectionManager _connectionManager = new ConnectionManager();
        private InputManager _inputManager;

        private Ship _player = GameModel.NullShip;
        internal Ship Player { get { return _player; } }

        private Dictionary<int, Sprite> _sprites = new Dictionary<int, Sprite>();

        public GameView()
        {
            _graphics = new GraphicsDeviceManager(this);
            _gui = new GraphicalUserInterface(this);
            _inputManager = new InputManager(_connectionManager);
            this.IsMouseVisible = true;
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
            GameModel.Initialize(this, _connectionManager, _inputManager);
            //SyncManager.Start();

            //_gui.Initialize();

            GameOver = false;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            SpaceBackground.LoadContent(Content);
            ViewHelper.LoadContent(Content);
            //_gui.LoadContent(Content);
            GameModel.LoadWorld();
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

            _inputManager.Update();

            GameModel.Update(gameTime);

            TimeRemaining -= (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (GameOver)
            {
                EndGame();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _player = _connectionManager.GetShip();
            if (_player == null)
            {
                return;
            }

            DrawBackground();
            DrawGameEntities();

            base.Draw(gameTime);
        }

        private void DrawGUI()
        {
            _spriteBatch.Begin();
            _gui.Draw(_spriteBatch);
            _spriteBatch.End();
        }

        private void DrawGameEntities()
        {
            _spriteBatch.Begin();
            foreach (Entity entityToDraw in GameModel.GameEntities)
            {
                if(entityToDraw.Renderer == null)
                {
                    List<Animation> animationCollection = ViewHelper.AnimationFactory(entityToDraw);
                    entityToDraw.Renderer = new Sprite(animationCollection, entityToDraw);
                }
                entityToDraw.Renderer.Draw(_spriteBatch, _player);
            }
            _spriteBatch.End();
        }

        private void DrawBackground()
        {
            _spriteBatch.Begin();
            SpaceBackground.Draw(_spriteBatch,
                (int)_player.Location.X,
                (int)_player.Location.Y,
                GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height
            );
            _spriteBatch.End();
        }

        internal void EndGame()
        {
            GameOver = true;
            
            //TODO: pause here for a key and add NewGame()
            //Console.ReadLine();
        }
    }
}
