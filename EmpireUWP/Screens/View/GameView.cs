using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using EmpireUWP.Model;
using EmpireUWP.Network;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace EmpireUWP.View
{
    public class GameView : Microsoft.Xna.Framework.Game
    {
        internal static Rectangle PlayArea = new Rectangle(0, 0, 20000, 20000);
        internal static Vector2 WindowSize = new Vector2(1280f, 720f);
        internal static Vector2 ViewCenter = new Vector2(WindowSize.X / 2, WindowSize.Y / 2);
        internal static int GameDuration = 1000 * 60 * 2;  // 2 minute timer in milliseconds
        internal GameData.GameStatus GameState = GameData.GameStatus.WaitingForPlayers;

        //TODO: fix scoring system
        internal int ShieldEnergy
        {
            get
            {
                if (_localShip.Owner != null)
                {
                    return _localShip.ShieldEnergy;
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
        private GameModel _gameModel;
        private ConnectionManager _connectionManager;

        private Ship _localShip = GameModel.NullShip;
        internal Ship LocalShip { get { return _localShip; } }

        private string PlayerID { get; set; }
        public string HostID { get; private set; }
        public bool Hosting { get { return HostID == PlayerID; } }
        
        private Dictionary<int, Sprite> _sprites = new Dictionary<int, Sprite>();

        public GameView()
        {
            _graphics = new GraphicsDeviceManager(this);
            _gui = new GraphicalUserInterface(this);
            _gameModel = new GameModel(this);

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

            SpaceBackground.Initialize();

            NewGame();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            SpaceBackground.LoadContent(Content);
            ViewHelper.LoadContent(Content);
        }

        protected override void UnloadContent()
        {
            SpaceBackground.UnloadContent();
            ViewHelper.UnloadContent();
        }

        protected override async void Update(GameTime gameTime)
        {

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            await _gameModel.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _localShip = _gameModel.GetShip(PlayerID);
            if (_localShip == null)
            {
                return;
            }

            DrawBackground();
            DrawGameEntities();

            base.Draw(gameTime);
        }

        private void DrawGameEntities()
        {
            _spriteBatch.Begin();
            foreach (Entity entityToDraw in _gameModel.GameEntities)
            {
                if(entityToDraw.Renderer == null)
                {
                    List<Animation> animationCollection = ViewHelper.AnimationFactory(entityToDraw);
                    entityToDraw.Renderer = new Sprite(animationCollection, entityToDraw);
                }
                entityToDraw.Renderer.Draw(_spriteBatch, _localShip);
            }
            _spriteBatch.End();
        }

        private void DrawBackground()
        {
            _spriteBatch.Begin();
            SpaceBackground.Draw(_spriteBatch,
                (int)_localShip.Location.X,
                (int)_localShip.Location.Y,
                GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height
            );
            _spriteBatch.End();
        }

        internal void GameChangedEventHandler(object sender, GameChangedEventArgs e)
        {
            //TODO:  Might be helpful to look into using the State pattern to handle game states and transitions between them.
            GameState = e.GameData.Status;
            if(GameState == GameData.GameStatus.ReadyToStart)
            {
                NewGame();
            }
        }

        private void NewGame()
        {
            if(_gameModel != null)
            {
                //_gameModel.Dispose();
            }
            _gameModel = new GameModel(this, _connectionManager);

            _gameModel.Initialize();
            if (Hosting || _connectionManager == null)
            {
                _gameModel.LoadWorld();
            }

            _gameModel.Start();

        }

        internal async Task StartGameServer(string playerID, Dictionary<string, PlayerData> playerList, GameData data)
        {
            HostID = data.HostID;
            PlayerID = playerID;
            _connectionManager = new ConnectionManager(PlayerID);
            await _connectionManager.StartHosting(playerList, data);
            _connectionManager.GameChanged += GameChangedEventHandler;
        }

        internal async Task StartGame(string playerID, string hostAddress)
        {
            PlayerID = playerID;

            if (_connectionManager == null) {
                _connectionManager = new ConnectionManager(PlayerID);
                _connectionManager.GameChanged += GameChangedEventHandler;
            }

            await _connectionManager.NotifyHost(hostAddress);
        }
    }
}
