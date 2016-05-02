using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Empire.Model;
using Empire.Network;
using System.Linq;
using System;

// TODO: Move the spites into a class of their own

namespace Empire.View
{
    public class GameView : Microsoft.Xna.Framework.Game
    {
        private static readonly log4net.ILog log = 
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        internal static Rectangle PlayArea = new Rectangle(0, 0, 20000, 20000);
        internal static Vector2 WindowSize = new Vector2(1280f, 760f);
        internal static Vector2 ViewCenter = new Vector2(WindowSize.X/2, WindowSize.Y/2);
        internal static int GameDuration = 1000 * 60 * 2;  // 2 minute timer in milliseconds

        internal bool GameOver { get; set; }
        internal int Score
        {
            get
            {
                if (_player != null)
                {
                    return _player.Score;
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
                if (_player != null)
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

        private Model.Ship _player;
        internal Model.Ship Player { get { return _player; } }

        private Dictionary<int, Sprite> _sprites = new Dictionary<int, Sprite>();
        private Dictionary<string, Animation> _animations = new Dictionary<string, Animation>();

        internal GameView()
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
            ConnectionManager.Initialize();
            GameModel.Initialize(this);
            SyncManager.Start();

            _gui.Initialize();

            GameOver = false;

            base.Initialize();
        }

        private void WaitForConnection()
        {
            while (ConnectionManager.ConnectionID==null)
            {
                ConnectionManager.Join("Mike");
                System.Threading.Thread.Sleep(1000);
            }
        }

        protected override void LoadContent()
        {
            SpaceBackground.LoadContent(Content);
            ViewHelper.LoadContent(Content);
            _gui.LoadContent(Content);
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

            if(ConnectionManager.ConnectionID == null)
            {
                System.Threading.Thread.Sleep(1000);
                WaitForConnection();
            }

            _player = ConnectionManager.GetShip();
            ProcessLocalInput();

            //_ai.Update();

            GameModel.Update(gameTime);
            //updateSprites(gameTime);
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

            int commands = 0;
            if (keyboardState.IsKeyDown(Keys.Left))
            {
                commands += (int)CommandFlags.Left;
            }
            if (keyboardState.IsKeyDown(Keys.Right))
            {
                commands += (int)CommandFlags.Right;
            }
            if (keyboardState.IsKeyDown(Keys.Up))
            {
                commands += (int)CommandFlags.Thrust;
            }
            if (keyboardState.IsKeyDown(Keys.Down))
            {
                commands += (int)CommandFlags.Shields;
            }
            if (keyboardState.IsKeyDown(Keys.Space))
            {
                commands += (int)CommandFlags.Shoot;
            }
            ConnectionManager.SendShipCommand(new ShipCommand(ConnectionManager.ConnectionID, commands));

        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            if(_player == null)
            {
                return;
            }

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
            _spriteBatch.Begin();
            foreach (Entity entity in GameModel.GameEntities)
            {
                entity.Renderer.Draw(_spriteBatch, _player);
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

        //internal void GenerateSprite(Entity entity)
        //{
        //    List<Animation> animationCollection = ViewHelper.AnimationFactory(entity);
        //    Sprite newSprite = new Sprite(animationCollection, entity);
        //    _sprites.Add(entity.EntityID, newSprite);
        //}

        internal void EndGame()
        {
            GameOver = true;
            
            //TODO: pause here for a key and add NewGame()
            Console.ReadLine();
        }
    }
}
