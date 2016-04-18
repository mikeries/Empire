using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire.Model
{
    class GameModel
    {
        private List<Entity> _gameEntities = new List<Entity>();
        public List<Entity> GameEntities { get { return _gameEntities; } }
        private Player _player;
        public Player Player { get { return _player; } }
        public static Random Random = new Random();

        KeyboardState currentKeyboardState;
        KeyboardState previousKeyboardState;
        private int _elapsedTime;
        private int _timeSinceLastShot;
        private int millisecondsPerRotation = 10;
        private const int millisecondsPerShot = 100;
        private int _score=0;
        private int _timeRemaining;

        public GameModel()
        {
            _player = new Player(20000,20000);
            _timeRemaining = View.Game.GameTime;
        }

        public void Initialize()
        {
            _gameEntities.Add(_player as Entity);
            _player.Height = 64;
            _player.Width = 64;
            _gameEntities.Add(ModelHelper.PlanetFactory(GameModel.Random.Next(10000, 30000), GameModel.Random.Next(10000, 30000), Planets.planet1) as Entity);
            _gameEntities.Add(ModelHelper.PlanetFactory(GameModel.Random.Next(10000, 30000), GameModel.Random.Next(10000, 30000), Planets.planet2) as Entity);
            _gameEntities.Add(ModelHelper.PlanetFactory(GameModel.Random.Next(10000, 30000), GameModel.Random.Next(10000, 30000), Planets.planet3) as Entity);
            _gameEntities.Add(ModelHelper.PlanetFactory(GameModel.Random.Next(10000, 30000), GameModel.Random.Next(10000, 30000), Planets.planet4) as Entity);
            _gameEntities.Add(ModelHelper.PlanetFactory(GameModel.Random.Next(10000, 30000), GameModel.Random.Next(10000, 30000), Planets.planet5) as Entity);
            _gameEntities.Add(ModelHelper.PlanetFactory(GameModel.Random.Next(10000, 30000), GameModel.Random.Next(10000, 30000), Planets.planet6) as Entity);
            _gameEntities.Add(ModelHelper.PlanetFactory(GameModel.Random.Next(10000, 30000), GameModel.Random.Next(10000, 30000), Planets.planet7) as Entity);
            //_gameEntities.Add(ModelHelper.PlanetFactory(GameModel.Random.Next(-1000, 1000), GameModel.Random.Next(-1000, 1000), Planets.planet8) as Entity);
            //_gameEntities.Add(ModelHelper.PlanetFactory(GameModel.Random.Next(-1000, 1000), GameModel.Random.Next(-1000, 1000), Planets.planet9) as Entity);
            //_gameEntities.Add(ModelHelper.PlanetFactory(GameModel.Random.Next(-1000, 1000), GameModel.Random.Next(-1000, 1000), Planets.planet10) as Entity);
            //_gameEntities.Add(ModelHelper.PlanetFactory(GameModel.Random.Next(-1000, 1000), GameModel.Random.Next(-1000, 1000), Planets.planet11) as Entity);
            //_gameEntities.Add(ModelHelper.PlanetFactory(GameModel.Random.Next(-1000, 1000), GameModel.Random.Next(-1000, 1000), Planets.planet12) as Entity);
            //_gameEntities.Add(ModelHelper.PlanetFactory(GameModel.Random.Next(-1000, 1000), GameModel.Random.Next(-1000, 1000), Planets.planet13) as Entity);
            //_gameEntities.Add(ModelHelper.PlanetFactory(GameModel.Random.Next(-1000, 1000), GameModel.Random.Next(-1000, 1000), Planets.planet14) as Entity);
            //_gameEntities.Add(ModelHelper.PlanetFactory(GameModel.Random.Next(-1000, 1000), GameModel.Random.Next(-1000, 1000), Planets.planet15) as Entity);
            //_gameEntities.Add(ModelHelper.PlanetFactory(GameModel.Random.Next(-1000, 1000), GameModel.Random.Next(-1000, 1000), Planets.planet16) as Entity);

            for (int i = 0; i < 4000; i++)
            {
                _gameEntities.Add(ModelHelper.AsteroidFactory(GameModel.Random.Next(View.Game.PlayArea.Left, View.Game.PlayArea.Right), GameModel.Random.Next(View.Game.PlayArea.Left, View.Game.PlayArea.Right)) as Entity);
                _gameEntities[_gameEntities.Count - 1].Velocity = new Vector2(GameModel.Random.Next(-300, 300)/100, Random.Next(-300, 300)/100);
            }

        }

        private static bool Collision(Circle c1, Circle c2)
        {
            return ((c1.Center - c2.Center).Length() < (c1.Radius + c2.Radius-1));  // the -1 is a fudge factor to be sure they actually collided.
        }

        public void Update(GameTime gameTime)
        {

            // First, update all entities and get rid of those that are done
            foreach (Entity entity in _gameEntities.ToList())
            {
                if (entity.Status == Status.Disposable)
                    _gameEntities.Remove(entity);
                else entity.Update(gameTime);
            }

            //// Use LINQ to pull out only the asteroids and check them against the non-asteroids.
            // also only selects those asteroids within 700 away from the player
            // this makes collision detection faster, but assumes that all collisionable objects (lasers and the ship) are
            // within this distance.
            var asteroids =
                from entity in _gameEntities
                where (entity is Asteroid && (entity.BoundingCircle.Center - _player.BoundingCircle.Center).Length() < 700)
                orderby entity.Location.X
                select entity;

            // next loop through again doing collision detection
            foreach (Entity entity in _gameEntities.ToList())
            {
                if (entity.Type != EntityType.Asteroid && entity.Type != EntityType.Planet) // non-asteroids can only collide with asteroids
                {
                    foreach(Entity asteroid in asteroids.ToList())
                    {
                        if (Collision(entity.BoundingCircle, asteroid.BoundingCircle))
                        {
                            HandleCollision(entity, asteroid);
                        }
                    }
                }
            }

            HandleInputs(gameTime);
        }

        private void HandleCollision(Entity entity, Entity struckAsteroid)
        {
            Asteroid asteroid = struckAsteroid as Asteroid;

            if (asteroid.Stage > 0)
            {
                for (int i = 0; i < Random.Next(3, 5); i++)
                {
                    // TODO: move this stuff elsewhere
                    Asteroid newAsteroid = new Asteroid((int)asteroid.Location.X, (int)asteroid.Location.Y);
                    float randomX = asteroid.Velocity.X + (float)Random.Next(-2000, 2000) / 1000;
                    float randomY = asteroid.Velocity.Y + (float)Random.Next(-2000, 2000) / 1000;
                    Vector2 vector = new Vector2(randomX, randomY);
                    newAsteroid.Velocity=vector;
                    int newSize = Random.Next(25, 75) * asteroid.Height / 100;
                    if (newSize < 20) newSize = 20;
                    newAsteroid.Height = newSize;
                    newAsteroid.Width = newSize;
                    newAsteroid.Stage = asteroid.Stage - 1;
                    newAsteroid.RollRate = Random.Next(-(1000 / newSize), (1000 / newSize));
                    newAsteroid.Style = asteroid.Style;
                    _gameEntities.Add(newAsteroid);
                }
            }
            asteroid.Status = Status.Dead;

            if (entity is Player && (((int)_player.visualState & (int)VisualStates.Shields)==0))
            {
                _score -= 100;
            } else if (!(entity is Player))
            {
                entity.Status = Status.Dead;
                _score += 10*(asteroid.Stage+1);
            }

            OnGameChanged("score",_score);
        }

        public void HandleInputs(GameTime gameTime)
        {

            _elapsedTime += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_elapsedTime > millisecondsPerRotation)
            {
                // Save the previous state of the keyboard and game pad so we can determine single key/button presses
                previousKeyboardState = currentKeyboardState;
                currentKeyboardState = Keyboard.GetState();

                _player.visualState = VisualStates.Idle; // reset visual state

                if (currentKeyboardState.IsKeyDown(Keys.Left)) _player.RotateShip(Direction.Left, _elapsedTime);
                if (currentKeyboardState.IsKeyDown(Keys.Right)) _player.RotateShip(Direction.Right, _elapsedTime);
                if (currentKeyboardState.IsKeyDown(Keys.Up)) _player.AccelerateShip(Direction.Up, _elapsedTime);

                if (currentKeyboardState.IsKeyDown(Keys.Down))
                {
                    _player.shieldEnergy -= (int)(_elapsedTime/4f);
                    if (_player.shieldEnergy > 1)_player.AccelerateShip(Direction.Down, _elapsedTime);
                }
                _player.shieldEnergy += (int)(_elapsedTime / 10f);
                _player.shieldEnergy = MathHelper.Clamp(_player.shieldEnergy, 0, 100);
                OnGameChanged("shieldEnergy", _player.shieldEnergy);

                if (currentKeyboardState.IsKeyDown(Keys.Space))
                {
                    _timeSinceLastShot += _elapsedTime;
                    if (_timeSinceLastShot > millisecondsPerShot) {
                        _gameEntities.Add(_player.Fire(gameTime));
                        _timeSinceLastShot = 0;
                    }
                }

                _timeRemaining -= _elapsedTime;
                OnGameChanged("timeRemaining", _timeRemaining);
                _elapsedTime = 0;
            }
        }

        public event EventHandler<GameEventArgs> GameChanged;
        private void OnGameChanged(string property, int value)
        {
            EventHandler<GameEventArgs> gameChanged = GameChanged;
            if (gameChanged != null)
            {
                gameChanged(this, new GameEventArgs(property,value));
            }
        }
    }
}
