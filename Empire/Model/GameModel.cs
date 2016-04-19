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
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private List<Entity> _gameEntities = new List<Entity>();
        public List<Entity> GameEntities { get { return _gameEntities; } }
        private Player _player;
        public Player Player { get { return _player; } }
        public static Random Random = new Random();

        KeyboardState currentKeyboardState;
        KeyboardState previousKeyboardState;

        private int _elapsedTime;
        private int _timeSinceLastShot;
        private const int millisecondsPerRotation = 10;     // minimum number of ms between rotations
        private const int millisecondsPerShot = 70;         // minimum number of ms between shots
        private const float ShieldDecayRate = 0.25f;        // shield decay rate per millisecond
        private const float ShieldRegenerationRate = 0.1f;  // shield regen rate per millisecond
        private const int CollisionCheckRange = 700;        // Objects must be at least this close to the player for collision checks

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
            _gameEntities.Add(ModelHelper.PlanetFactory(GameModel.Random.Next(10000, 30000), GameModel.Random.Next(10000, 30000), Planets.planet8) as Entity);
            _gameEntities.Add(ModelHelper.PlanetFactory(GameModel.Random.Next(10000, 30000), GameModel.Random.Next(10000, 30000), Planets.planet9) as Entity);
            _gameEntities.Add(ModelHelper.PlanetFactory(GameModel.Random.Next(10000, 30000), GameModel.Random.Next(10000, 30000), Planets.planet10) as Entity);
            _gameEntities.Add(ModelHelper.PlanetFactory(GameModel.Random.Next(10000, 30000), GameModel.Random.Next(10000, 30000), Planets.planet11) as Entity);
            _gameEntities.Add(ModelHelper.PlanetFactory(GameModel.Random.Next(10000, 30000), GameModel.Random.Next(10000, 30000), Planets.planet12) as Entity);
            _gameEntities.Add(ModelHelper.PlanetFactory(GameModel.Random.Next(10000, 30000), GameModel.Random.Next(10000, 30000), Planets.planet13) as Entity);
            _gameEntities.Add(ModelHelper.PlanetFactory(GameModel.Random.Next(10000, 30000), GameModel.Random.Next(10000, 30000), Planets.planet14) as Entity);
            _gameEntities.Add(ModelHelper.PlanetFactory(GameModel.Random.Next(10000, 30000), GameModel.Random.Next(10000, 30000), Planets.planet15) as Entity);
            _gameEntities.Add(ModelHelper.PlanetFactory(GameModel.Random.Next(10000, 30000), GameModel.Random.Next(10000, 30000), Planets.planet16) as Entity);

            for (int i = 0; i < 8000; i++)
            {
                _gameEntities.Add(ModelHelper.AsteroidFactory(GameModel.Random.Next(View.Game.PlayArea.Left, View.Game.PlayArea.Right), GameModel.Random.Next(View.Game.PlayArea.Left, View.Game.PlayArea.Right)) as Entity);
                _gameEntities[_gameEntities.Count - 1].Velocity = new Vector2(GameModel.Random.Next(-300, 300)/100, Random.Next(-300, 300)/100);
            }

        }

        private static bool Collision(Circle c1, Circle c2)
        {
            return ((c1.Center - c2.Center).Length() < (c1.Radius + c2.Radius-1));
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

            // Pull out only the asteroids and check them against the non-asteroids.
            // also only selects those asteroids within CollisionCheckRange away from the player
            // this makes collision detection faster, but assumes that all collisionable objects (lasers and the ship) are
            // within this distance.
            var asteroids =
                from entity in _gameEntities
                where (entity is Asteroid && 
                    (entity.BoundingCircle.Center - _player.BoundingCircle.Center).Length() < CollisionCheckRange)
                select entity;

            // next loop through again doing collision detection
            foreach (Entity entity in _gameEntities.ToList())
            {
                if (entity.Type != EntityType.Asteroid && 
                    entity.Type != EntityType.Planet) // non-asteroids can only collide with asteroids
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

        private void HandleCollision(Entity entity, Entity entityThatCollided)
        {

            if (entityThatCollided is Asteroid)
            {
                Asteroid asteroid = entityThatCollided as Asteroid;

                if (asteroid.Stage > 0)
                {
                    for (int i = 0; i < Random.Next(3, 5); i++)
                    {
                        _gameEntities.Add(asteroid.childAsteroid());
                    }
                }
                asteroid.Status = Status.Dead;

                if (entity is Player && ShieldsAreDown())
                {
                    _score -= 100;
                }
                else if (!(entity is Player))
                {
                    {
                        entity.Status = Status.Dead;
                        _score += 10 * (asteroid.Stage + 1);
                    }
                }
                OnGameChanged("score", _score);
            }
        }

        private bool ShieldsAreDown()
        {
            return ((int)_player.visualState & (int)VisualStates.Shields) == 0;
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
                    _player.ShieldEnergy -= (int)(_elapsedTime*ShieldDecayRate);
                    if (_player.ShieldEnergy > 1)_player.AccelerateShip(Direction.Down, _elapsedTime);
                }
                _player.ShieldEnergy += (int)(_elapsedTime * ShieldRegenerationRate);
                _player.ShieldEnergy = MathHelper.Clamp(_player.ShieldEnergy, 0, 100);
                OnGameChanged("shieldEnergy", _player.ShieldEnergy);

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
