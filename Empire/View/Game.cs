using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Empire.Model;
using System.Linq;


namespace Empire.View
{
    public class Game : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpaceBackground spaceBackground = new SpaceBackground();
        public static Rectangle PlayArea = new Rectangle(0, 0, 40000, 40000);
        public static Vector2 ViewCenter = new Vector2(640f,360f);
        public static int GameTime = 1000 * 60 * 2;  // 2 minute timer

        private Model.GameModel _model = new Model.GameModel();
        private Model.Player _player;

        private Dictionary<Entity, Sprite> sprites = new Dictionary<Entity, Sprite>();
        private Dictionary<string, Animation> _animations = new Dictionary<string, Animation>();
        private Dictionary<string, IGUIElement> _gui = new Dictionary<string, IGUIElement>();

        private bool _gameOver { get; set; }

        public Game()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            _model.GameChanged += GameChangedEventHandler;
        }

        private void GameChangedEventHandler(object sender, GameEventArgs e)
        {
            if (e==null || _gameOver) return;
            if (_gui.ContainsKey(e.Property)) {

                switch (e.Property)
                {
                    case "score":
                        TextBlock scoreText = _gui["score"] as TextBlock;
                        scoreText.Text = e.Value.ToString();
                        break;
                    case "shieldEnergy":
                        StatusBar bar = _gui["shieldEnergy"] as StatusBar;
                        bar.Value = e.Value / 100f;
                        break;
                    case "timeRemaining":
                        bar = _gui["timeRemaining"] as StatusBar;
                        bar.Value = (float)e.Value / (float)GameTime;
                        if (e.Value < 0) GameOver();  // Oh nuts!
                        break;
                }

            }
        }

        private void GameOver()
        {
            _gui["gameOver"].Visible = true;
            _gameOver = true;
            // need to pause here for a key press...
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferHeight = 720;
            graphics.PreferredBackBufferWidth = 1280;
            graphics.ApplyChanges();
            spaceBackground.Initialize();

            _model.Initialize();
            _player = _model.Player;

            TextBlock score = new TextBlock("score", "Score24", 1100, 10);
            _gui.Add(score.Name, score);
            score.Text = "0";

            StatusBar shieldEnergy = new StatusBar("shieldEnergy", 10, 10);
            shieldEnergy.Name = "shieldEnergy";
            shieldEnergy.BackColor = Color.Red;
            shieldEnergy.ForeColor = Color.GreenYellow;
            shieldEnergy.Value = 0.8f;
            shieldEnergy.Height = 20;
            shieldEnergy.Width = 200;
            _gui.Add(shieldEnergy.Name, shieldEnergy);

            TextBlock label = new TextBlock("shieldLabel", "Arial12", 220, 12);
            label.Text = "Shield Energy";
            _gui.Add(label.Name, label);

            StatusBar timer = new StatusBar("timeRemaining", 10, 40);
            timer.Name = "timeRemaining";
            timer.BackColor = Color.Red;
            timer.ForeColor = Color.GreenYellow;
            timer.Value = 1f;
            timer.Height = 20;
            timer.Width = 200;
            _gui.Add(timer.Name, timer);

            label = new TextBlock("timerLabel", "Arial12", 220, 42);
            label.Text = "Time Remaining";
            _gui.Add(label.Name, label);

            label = new TextBlock("gameOver", "Score24", 450, 200);
            label.Text = "            Game Over\n\nPress any key to play again.";
            label.Color = Color.Crimson;
            label.Visible = false;
            _gui.Add(label.Name, label);

            _gameOver = false;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spaceBackground.LoadContent(Content);
            ViewHelper.LoadContent(Content);

            foreach (string guiElement in _gui.Keys)
                _gui[guiElement].Initialize();          // note that Initialize() is called here because the GUI needs textures
        }

        protected override void UnloadContent()
        {
            spaceBackground.UnloadContent();
            ViewHelper.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _model.Update(gameTime);
            _player.Update(gameTime);
            updateSprites(gameTime);
            foreach (string guiElement in _gui.Keys)
                    _gui[guiElement].Update(gameTime);         
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(SpriteSortMode.FrontToBack);
            spaceBackground.Draw(spriteBatch, (int)_player.X,(int)_player.Y, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

            foreach(Sprite sprite in sprites.Values)
            {
                sprite.Draw(spriteBatch, _player.Location);
            }

            spriteBatch.End();

            spriteBatch.Begin();
            foreach (string guiElement in _gui.Keys)
                _gui[guiElement].Draw(spriteBatch);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void updateSprites(GameTime gameTime)
        {

            // remove dead ones
            var deadEntities =
                from entity in _model.GameEntities
                where entity.Status == Status.Dead
                select entity;
            foreach (Entity entity in deadEntities)
            {
                sprites.Remove(entity);
                entity.Status = Status.Disposable;
            }

            // add new ones
            var newEntities =
                from entity in _model.GameEntities
                where entity.Status == Status.New
                select entity;
            foreach(Entity entity in newEntities)
            {
                List<Animation> animationCollection = ViewHelper.AnimationFactory(entity);
                Sprite newSprite = new Sprite(animationCollection, entity);
                sprites.Add(entity, newSprite);
                entity.Status = Status.Active;
            }

            // update all
            foreach (Sprite sprite in sprites.Values)
                sprite.Update(gameTime);
        }

    }
}
