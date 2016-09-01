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
        internal GameData.GameStatus GameState = GameData.GameStatus.WaitingForPlayers;
        internal bool Hosting
        { get
            {
            if (GameClientConnection == null) return false;
            else return GameClientConnection.Hosting;
            }
        }

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

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        internal GameModel GameModel;
        internal GameClient GameClientConnection;
        internal GameServer GameServerConnection = null;

        private Ship _localShip = null;
        internal Ship LocalShip { get { return _localShip; } }

        private string PlayerID { get; set; }
        
        private Dictionary<int, Sprite> _sprites = new Dictionary<int, Sprite>();

        public GameView()
        {
            _graphics = new GraphicsDeviceManager(this);
            GameModel = new GameModel(this);

            this.IsMouseVisible = true;
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
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

            GameModel.Update(gameTime);

            await Task.Delay(0);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            if (GameClientConnection != null)
            {
                _localShip = GameClientConnection.GetShip(PlayerID);
            } 

            if(_localShip == null)
            {
                _localShip = GameModel.NullShip;
            }

            DrawBackground();
            DrawGameEntities();

            base.Draw(gameTime);
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
            GameState = e.GameData.Status;
            if(GameState == GameData.GameStatus.ReadyToStart)
            {
                NewGame();
            }
        }

        private void NewGame()
        {
            if (GameModel != null)
            {
                GameModel.Dispose();
            }
            GameModel = new GameModel(this);
            GameModel.Initialize();
        }

        internal Task StartServer(string playerID, Dictionary<string, PlayerData> playerList, GameData gameData)
        {
            PlayerID = playerID;
            gameData.HostIPAddress = playerList[gameData.HostID].IPAddress;
            gameData.HostPort = NetworkPorts.GameServerPort;
            GameServerConnection = new GameServer(this, playerList, gameData);
            return GameServerConnection.StartServer();
        }

        internal async Task StartGame(string playerID, GameData gameData)
        {
            PlayerID = playerID;

            if (GameClientConnection == null) {
                GameClientConnection = new GameClient(this, gameData, PlayerID, NetworkPorts.GameClientPort);
                await GameClientConnection.CreateNetworkConnection();
                GameClientConnection.GameChanged += GameChangedEventHandler;
            }

            NewGame();
            await GameClientConnection.ConnectToServer();

            await MenuManager.LeaveLobby();
        }
    }
}

