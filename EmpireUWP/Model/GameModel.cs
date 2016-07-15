using EmpireUWP.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using EmpireUWP.View;
using System.Collections.Concurrent;

namespace EmpireUWP.Model
{
    class GameModel
    {
        public static Random Random = new Random();

        // all the game entities that exist
        private ConcurrentDictionary<int,Entity> _gameEntities = new ConcurrentDictionary<int, Entity>();
        public List<Entity> GameEntities { get {return _gameEntities.Values.ToList(); } }
        public List<int> GameEntityIDs { get { return _gameEntities.Keys.ToList(); } }
        private static Ship _nullShip;
        public static Ship NullShip { get { return _nullShip; } }

        private GameView _game;
        internal WorldData worldData;
        private ConnectionManager _connectionManager;
        private InputManager _inputManager;
        private SyncManager _syncManager;

        public GameModel(GameView game, ConnectionManager connection = null)
        {
            _game = game;
            _connectionManager = connection;
            if (_connectionManager != null) _inputManager = new InputManager(_connectionManager);
            _syncManager = new SyncManager(this);
            _nullShip = new NullShip(this);
            worldData = new WorldData();
        }

        public void Initialize()
        {
            if (_connectionManager != null) _connectionManager.Initialize(this);
            worldData.Initialize(this,_inputManager);
        }

        // a basic world initialization.  This needs to be replaced with
        // a class that can generate a new 'world' as well as saving and restoring one.
        internal void LoadWorld()
        {
            for (int i = 0; i < 1; i++)
            {
                Planet planet = worldData.PlanetFactory(new Vector2(Random.Next(-10000, 10000), Random.Next(-10000, 10000)), (Planets)i);
                addGameEntityFromHost(planet);
            }

            for (int i = 0; i < WorldData.InitialAsteroidCount; i++)
            {
                Asteroid asteroid = worldData.AsteroidFactory();
                asteroid.Velocity = new Vector2(Random.Next(-200, 200) / 1000f, Random.Next(-200, 200) / 1000f);
                asteroid.Location = new Vector2(Random.Next(GameView.PlayArea.Left, GameView.PlayArea.Right),
                    Random.Next(GameView.PlayArea.Left, GameView.PlayArea.Right));
                addGameEntityFromHost(asteroid);
            }

        }

        public async Task Update(GameTime gameTime)
        {
            int elapsedTime = (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_inputManager != null)
                await _inputManager.Update();

            if (!_game.Hosting)
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

            if (_game.Hosting)
            {
                var asteroids =
                    from entity in _gameEntities.Values
                    where (entity is Asteroid)
                    select entity;
                DetectCollisionsWith(asteroids);
            }
        }

        private void RemoveDeadEntities(List<Entity> deadEntities)
        {
            if (_game.Hosting)
            {
                OnEntitiesRemoved(deadEntities);
            }

            foreach (Entity entity in deadEntities)
            {
                Entity entityToRemove = entity;
                if (!_gameEntities.TryRemove(entity.EntityID, out entityToRemove))
                {
                    //log.Warn("Failed to remove a game entity from the collection");
                }
                worldData.ReturnToPool(entityToRemove);
            }
        }

        private void DetectCollisionsWith(IEnumerable<Entity> asteroids)
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

        private bool Collision(Entity e1, Entity e2)
        {
            return ((e1.Location - e2.Location).Length() < (e1.Radius + e2.Radius));
        }

        internal Entity GetEntity(int entityID)
        {
            if (_gameEntities.ContainsKey(entityID))
            {
                return _gameEntities[entityID];
            }
            //log.Warn("Unknown entity request from GameModel");
            return null;
        }

        internal void Start()
        {
            _syncManager.Start(_connectionManager);
        }

        internal void Dispose()
        {
            _game = null;
            _connectionManager = null;
            worldData = null;
            _inputManager = null;
            _syncManager = null;
        }

        internal Ship GetShip(string owner)
        {
            if (_connectionManager == null)
            {
                return NullShip;
            }
            else
            {
                return _connectionManager.GetShip(owner);
            }
        }

        internal int NewShip(string playerID)
        {
            Ship ship = worldData.ShipFactory();
            ship.Owner = playerID;
            addGameEntityFromHost(ship);
            return ship.EntityID;
        }

        internal void AddGameEntity(Entity entity)
        {
            if(_game.Hosting)
            {
                addGameEntityFromHost(entity);
            }
            else
            {
                worldData.ReturnToPool(entity);
            }
        }
        
        private void addGameEntityFromHost(Entity entity)
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
                //log.Warn("Null entity attempted to be added to the game.");
            }
        }

        private void ApplyUpdates()
        {
            UpdateQueue queue = _syncManager.RetrieveUpdatesAndClear();

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
                    Entity updatedEntity = worldData.EntityFactory(packet.EntityType, packet.EntityState);
                    addGameEntityFromHost(updatedEntity);
                }
            }
        }

        public event EventHandler<EntitiesRemovedEventAgs> EntitiesRemoved = delegate { };
        private void OnEntitiesRemoved(List<Entity> deadEntities)
        {
            EntitiesRemoved?.Invoke(null, new EntitiesRemovedEventAgs(deadEntities));
        }
    }
}
