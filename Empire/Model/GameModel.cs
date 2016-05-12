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
using System.Collections.Concurrent;

namespace Empire.Model
{
    class GameModel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static Random Random = new Random();

        // all the game entities that exist
        private static ConcurrentDictionary<int,Entity> _gameEntities = new ConcurrentDictionary<int, Entity>();
        public static List<Entity> GameEntities { get {return _gameEntities.Values.ToList(); } }
        public static List<int> GameEntityIDs { get { return _gameEntities.Keys.ToList(); } }
        private static GameView _game;

        public static void Initialize(GameView game)
        {
            _game = game;

            ModelHelper.Initialize();
        }

        // a basic world initialization.  This needs to be replaced with
        // a class that can generate a new 'world' as well as saving and restoring one.
        internal static void LoadWorld()
        {
            if (ConnectionManager.IsHost)
            {

                for (int i = 0; i < 1; i++)
                {
                    Planet planet = ModelHelper.PlanetFactory(new Vector2(Random.Next(-10000, 10000), Random.Next(-10000, 10000)), (Planets)i);
                    addGameEntityFromHost(planet);
                }

                for (int i = 0; i < ModelHelper.InitialAsteroidCount; i++)
                {
                    Asteroid asteroid = ModelHelper.AsteroidFactory();
                    asteroid.Velocity = new Vector2(Random.Next(-200, 200) / 1000f, Random.Next(-200, 200) / 1000f);
                    asteroid.Location = new Vector2(GameModel.Random.Next(View.GameView.PlayArea.Left, View.GameView.PlayArea.Right),
                        GameModel.Random.Next(View.GameView.PlayArea.Left, View.GameView.PlayArea.Right));
                    addGameEntityFromHost(asteroid);
                }
            }
        }

        public static void Update(GameTime gameTime)
        {
            int elapsedTime = (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (!ConnectionManager.IsHost)
            {
                ApplyUpdates();
            }

            List<Entity> deadEntities = new List<Entity>();
            foreach (Entity entity in _gameEntities.Values)
            {
                if (entity.Status == Status.Disposable)
                {
                    deadEntities.Add(entity);
                }
                else
                {
                    entity.Update(elapsedTime);
                }
            }

            RemoveDeadEntities(deadEntities);

            if (ConnectionManager.IsHost)
            {
                var asteroids =
                    from entity in _gameEntities.Values
                    where (entity is Asteroid)
                    select entity;
                DetectCollisionsWith(asteroids);
            }
        }

        private static void RemoveDeadEntities(List<Entity> deadEntities)
        {
            if (ConnectionManager.IsHost)
            {
                SyncManager.RemoveEntities(deadEntities);
            }

            foreach (Entity entity in deadEntities)
            {
                Entity entityToRemove = entity;
                if (!_gameEntities.TryRemove(entity.EntityID, out entityToRemove))
                {
                    log.Warn("Failed to remove a game entity from the collection");
                }
                ModelHelper.ReturnToPool(entityToRemove);
            }
        }

        private static void DetectCollisionsWith(IEnumerable<Entity> asteroids)
        {
            foreach (Entity entity in _gameEntities.Values.ToList())
            {
                if (entity.Type != EntityType.Asteroid &&
                    entity.Type != EntityType.Planet) // non-asteroids can only collide with asteroids
                {
                    foreach (Entity asteroid in asteroids.ToList())
                    {
                        if (Collision(entity, asteroid))
                        {
                            entity.HandleCollision(asteroid);
                            asteroid.HandleCollision(entity);
                        }
                    }
                }
            }
        }

        private static bool Collision(Entity e1, Entity e2)
        {
            return ((e1.Location - e2.Location).Length() < (e1.Radius + e2.Radius));
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

        internal static int NewShip(string connectionID)
        {
            Ship ship = ModelHelper.ShipFactory();
            ship.Owner = connectionID;
            addGameEntityFromHost(ship);
            return ship.EntityID;
        }

        internal static void AddGameEntity(Entity entity)
        {
            if(ConnectionManager.IsHost)
            {
                addGameEntityFromHost(entity);
            }
            else
            {
                ModelHelper.ReturnToPool(entity);
            }
        }
        
        private static void addGameEntityFromHost(Entity entity)
        {
            if (entity != null)
            {
                if (entity.EntityID == 0)
                {
                    entity.GenerateID();
                }

                entity.Status = Status.Active;
                _gameEntities.TryAdd(entity.EntityID, entity);                
            } else
            {
                log.Warn("Null entity attempted to be added to the game.");
            }
        }

        private static void ApplyUpdates()
        {
            UpdateQueue queue = SyncManager.RetrieveUpdatesAndClear();

            foreach (EntityPacket packet in queue.Packets)
            {

                if (_gameEntities.ContainsKey(packet.EntityID))
                {
                    Entity entityToReplace = _gameEntities[packet.EntityID];
                    entityToReplace.SetState(packet.EntityState);
                    TimeSpan lag = DateTime.Now - entityToReplace.LastUpdated;
                    entityToReplace.Update((int)lag.TotalMilliseconds);
                }
                else
                {
                    Entity updatedEntity = ModelHelper.EntityFactory(packet.EntityType, packet.EntityState);
                    addGameEntityFromHost(updatedEntity);
                }
            }
        }
    }
}
