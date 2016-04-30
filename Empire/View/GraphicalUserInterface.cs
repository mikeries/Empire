using Empire.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire.View
{
    // The Graphical User Interface class is responsible for creating and updating the
    // elements of the user interface that are not part of the game world.  This includes
    // status bars, text boxes, minimap, etc.
    class GraphicalUserInterface
    {
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        // collection of gui elements
        private Dictionary<string, IGUIElement> _gui = new Dictionary<string, IGUIElement>();
        private GameView _gameManager;

        public GraphicalUserInterface(GameView gameManager)
        {
            _gameManager = gameManager;

            // hook up the model's event handlers
            //GameModel.GameChanged += GameChangedEventHandler;
        }

        internal void Initialize()
        {
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
        }

        internal void LoadContent(ContentManager content)
        {
            foreach (string guiElement in _gui.Keys)
            {
                // note that _gui.Initialize() is called here instead of because the GUI needs textures to be loaded first
                _gui[guiElement].LoadContent();
            }
        }

        internal void Update(GameTime gameTime)
        {
            TextBlock scoreText = _gui["score"] as TextBlock;
            scoreText.Text = _gameManager.Score.ToString();

            StatusBar shieldBar = _gui["shieldEnergy"] as StatusBar;
            shieldBar.Value = _gameManager.ShieldEnergy / 100f;

            StatusBar timeBar = _gui["timeRemaining"] as StatusBar;
            timeBar.Value = _gameManager.TimeRemaining / (float)GameView.GameDuration;
            if (_gameManager.TimeRemaining < 0)
            {
                _gameManager.GameOver = true;
                _gui["gameOver"].Visible = true;
            }

            foreach (string guiElement in _gui.Keys)
                _gui[guiElement].Update(gameTime);
        }

        internal void Draw(SpriteBatch spriteBatch)
        {
            foreach (string guiElement in _gui.Keys)
            {
                _gui[guiElement].Draw(spriteBatch);
            }
        }
        
        // Update GUI elements when the model indicates something changed
        private void GameChangedEventHandler(object sender, GameEventArgs e)
        {
            if (e == null || _gameManager.GameOver) return;

            if (_gui.ContainsKey(e.Property))
            {
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
                        bar.Value = (float)e.Value / (float)GameView.GameDuration;
                        if (e.Value < 0)
                        {
                            _gameManager.GameOver = true;
                            _gui["gameOver"].Visible = true;
                        }
                        break;
                    default:
                        log.Warn("GameModel threw unknown event.");
                        break;
                        
                } // case
            } // if
        }
    }
}
