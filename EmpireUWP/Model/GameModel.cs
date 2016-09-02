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
    public class GameModel
    {
        public static Random Random = new Random();

        // all the game entities that exist
        private ConcurrentDictionary<int,Entity> _gameEntities = new ConcurrentDictionary<int, Entity>();
        public List<Entity> GameEntities { get {return _gameEntities.Values.ToList(); } }

        private Ship _nullShip;
        public Ship NullShip { get { return _nullShip; } }

        private GameView _gameInstance;
        internal WorldData worldData;
        internal InputManager InputManager;

        public GameModel(GameView gameInstance)
        {
            _gameInstance = gameInstance;
            _nullShip = new NullShip(this);
            worldData = new WorldData();
        }

        public void Initialize()
        {
            if (_gameInstance.GameClientConnection != null)
            {
                InputManager = new InputManager(_gameInstance.GameClientConnection);
            }
            worldData.Initialize(this,InputManager);
            if(_gameInstance.GameState == GameData.GameStatus.WaitingForPlayers || _gameInstance.Hosting)
            {
                LoadWorld();
            }
        }

        // a basic world initialization.  This needs to be replaced with
        // a class that can generate a new 'world' as well as saving and restoring one.
        public void LoadWorld()
        {
            for (int i = 0; i < 10; i++)
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

        public void Update(GameTime gameTime)
        {
            int elapsedTime = (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (InputManager != null)
            {
                InputManager.Update();
            }

            if (!_gameInstance.Hosting)
            {
                if (_gameInstance.GameClientConnection != null)
                {
                    ApplyUpdates();
                }
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

            if (_gameInstance.Hosting)
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
            if (_gameInstance.Hosting)
            {
                OnEntitiesRemoved(deadEntities);
            }

            foreach (Entity entity in deadEntities)
            {
                Entity entityToRemove = entity;
                if (!_gameEntities.TryRemove(entity.EntityID, out entityToRemove))
                {
                    throw new Exception("Failed to remove a game entity from the collection");
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

        public Entity GetEntity(int entityID)
        {
            if (_gameEntities.ContainsKey(entityID))
            {
                return _gameEntities[entityID];
            }
            //log.Warn("Unknown entity request from GameModel");
            return null;
        }

        public void Dispose()
        {
            _gameInstance = null;
            worldData = null;
            InputManager = null;
        }

        public int NewShip(string playerID)
        {
            Ship ship = worldData.ShipFactory();
            ship.Owner = playerID;
            addGameEntityFromHost(ship);
            return ship.EntityID;
        }

        public void AddGameEntity(Entity entity)
        {
            if(_gameInstance.Hosting)
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
                throw new Exception("Null entity attempted to be added to the game.");
            }
        }

        private void ApplyUpdates()
        {
            UpdateQueue queue = _gameInstance.GameClientConnection.RetrieveUpdatesAndClear();

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
