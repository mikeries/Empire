using Empire.Network;
using Empire.Network.PacketTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Empire.View;

namespace Empire.Model
{
    class GameModel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // all the game entities that exist
        private static Dictionary<int,Entity> _gameEntities = new Dictionary<int, Entity>();
        public static List<Entity> GameEntities { get { return _gameEntities.Values.ToList(); } }

        private static GameView _game;

        private static List<Entity> _entityUpdates = new List<Entity>();

        private static List<Ship> _ships;
        public static List<Ship> Ships
        {
            get
            {
                var ships =
                    from entity in _gameEntities.Values
                    where (entity is Ship)
                    select entity;

                _ships = new List<Ship>();
                foreach (Ship ship in ships.ToList())
                {
                    _ships.Add(ship);
                }

                return _ships as List<Ship>;
            }
        }

        public static Random Random = new Random();
        public const int InitialAsteroidCount = 2000;
        public const int InitialShipCount = 2;

        // a basic world initialization.  This needs to be replaced with
        // a class that can generate a new 'world' as well as saving and restoring one.
        public static void Initialize(GameView game)
        {
            _game = game;
        }

        internal static void LoadWorld()
        {
            if (ConnectionManager.IsHost)
            {

                for (int i = 0; i < (int)Planets.Count; i++)
                {
                    Planet planet = ModelHelper.SpawnPlanet(new Vector2(Random.Next(-10000, 10000), Random.Next(-10000, 10000)), (Planets)i);
                    addGameEntityFromHost(planet);
                }

                for (int i = 0; i < InitialAsteroidCount; i++)
                {
                    Asteroid asteroid = ModelHelper.SpawnAsteroid(new Vector2(GameModel.Random.Next(View.GameView.PlayArea.Left, View.GameView.PlayArea.Right),
                        GameModel.Random.Next(View.GameView.PlayArea.Left, View.GameView.PlayArea.Right)));
                    asteroid.Velocity = new Vector2(Random.Next(-200, 200) / 1000f, Random.Next(-200, 200) / 1000f);
                    addGameEntityFromHost(asteroid);
                }
            }
        }

        private static bool Collision(Circle c1, Circle c2)
        {
            return ((c1.Center - c2.Center).Length() < (c1.Radius + c2.Radius-1));
        }

        public static void Update(GameTime gameTime)
        {
            // first apply any updates we got from the host
            if (!ConnectionManager.IsHost)
            {
                ApplyUpdates();
            }
            else
            {
                //SyncManager.Sync();
            }

            // First, update all entities and get rid of those that are done
            foreach (Entity entity in _gameEntities.Values.ToList())
            {
                if (entity.Status == Status.Disposable)
                    _gameEntities.Remove(entity.EntityID);
                else entity.Update(gameTime);
            }

            // Pull out only the asteroids and check them against the non-asteroids.
            var asteroids =
                from entity in _gameEntities.Values
                where (entity is Asteroid)
                select entity;

            // next loop through again doing collision detection
            foreach (Entity entity in _gameEntities.Values.ToList())
            {
                if (entity.Type != EntityType.Asteroid && 
                    entity.Type != EntityType.Planet) // non-asteroids can only collide with asteroids
                {
                    foreach(Entity asteroid in asteroids.ToList())
                    {
                        if (Collision(entity.BoundingCircle, asteroid.BoundingCircle))
                        {
                            entity.HandleCollision(asteroid);
                            asteroid.HandleCollision(entity);
                        }
                    }
                }
            }

        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        internal static Ship GetShip(string connectionID)
        {
            var ships =
                from entity in _gameEntities.Values
                where (entity is Ship)
                select entity;

            foreach(Ship s in ships.ToList())
            {
                if (s.Owner == connectionID)
                {
                    return s;               // early exit
                }
            }

            return null;
        }

        internal static Entity GetEntity(int entityID)
        {
            if (_gameEntities.ContainsKey(entityID))
            {
                return _gameEntities[entityID];
            }
            log.Warn("Unknown entity request from GameModel");
            return null;
        }

        internal static void NewShip(string connectionID)
        {
            Ship ship = new Ship(new Vector2(View.GameView.PlayArea.Width / 2, View.GameView.PlayArea.Height / 2));
            ship.Owner = connectionID;
            addGameEntityFromHost(ship);
        }

        internal static void AddGameEntity(Entity entity)
        {
            if(ConnectionManager.IsHost)
            {
                addGameEntityFromHost(entity);
            }
        }
        
        private static void addGameEntityFromHost(Entity entity)
        {
            if (entity != null)
            {
                List<Animation> animationCollection = ViewHelper.AnimationFactory(entity);
                entity.Renderer = new Sprite(animationCollection, entity);
                if (entity.EntityID == 0)
                {
                    entity.GenerateID();
                }
                entity.Status = Status.Active;
                _gameEntities.Add(entity.EntityID, entity);
            }
        }

        public static void ApplyUpdates()
        {
            UpdateQueue queue = SyncManager.RetrieveUpdatesAndClear();

            foreach (EntityPacket packet in queue.Packets)
            {
                TimeSpan lag = DateTime.Now - packet.Timestamp;
                Entity entity = packet.EnclosedEntity;

                if (_gameEntities.ContainsKey(entity.EntityID))
                {
                    // Replace the object, but copy over the Renderer and point it at this object
                    Sprite renderer = _gameEntities[entity.EntityID].Renderer;
                    renderer.SetEntity(entity);
                    _gameEntities[entity.EntityID] = entity;
                    _gameEntities[entity.EntityID].Renderer = renderer;
                }
                else
                {
                    addGameEntityFromHost(entity);
                }
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        internal static void UpdateEntity(Entity entity)
        {
            _entityUpdates.Add(entity);
        }

    }
}
