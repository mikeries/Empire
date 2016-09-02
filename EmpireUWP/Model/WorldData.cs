using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using EmpireUWP.View;

namespace EmpireUWP.Model
{
    public class WorldData
    {

        const int PlanetMinSize = 150;
        const int PlanetMaxSize = 250;
        const int AsteroidMinSize = 100;
        const int AsteroidMaxSize = 150;
        const int AsteroidInitialStage = 2;
        const int LaserHeight = 3;
        const int LaserWidth = 3;
        const float LaserSpeed = 1.0f;
        public const int InitialAsteroidCount = 1000;
        public const int InitialShipCount = 1;

        private EntityPool<Asteroid> asteroidPool;
        private EntityPool<Laser> laserPool;
        private EntityPool<Planet> planetPool;
        private EntityPool<Ship> shipPool;
        private InputManager _inputManager;
        private GameModel _gameModel;

        public void Initialize(GameModel gameModel, InputManager inputManager)
        {
            asteroidPool = new EntityPool<Asteroid>(() => { return new Asteroid(gameModel); }, 3*InitialAsteroidCount);
            laserPool = new EntityPool<Laser>(() => { return new Laser(gameModel); }, 300);
            shipPool = new EntityPool<Ship>(() => { return new Ship(gameModel); }, 8 * InitialShipCount);
            planetPool = new EntityPool<Planet>(() => { return new Planet(gameModel); }, 3*(int)Planets.Count );

            _gameModel = gameModel;
            _inputManager = inputManager;
        }

        public Ship ShipFactory()
        {
            Ship newShip = shipPool.GetNew() as Ship;
            newShip.Location = new Vector2(View.GameView.PlayArea.Width / 2, View.GameView.PlayArea.Height / 2);
            newShip.Renderer = null;
            _inputManager.CommandReceived -= newShip.CommandReceivedHandler;
            _inputManager.CommandReceived += newShip.CommandReceivedHandler;
            return newShip;
        }

        public Planet PlanetFactory(Vector2 location, Planets ID)
        {
            Planet newPlanet = planetPool.GetNew() as Planet;
            newPlanet.PlanetID = ID;
            newPlanet.Location = location;

            int size = GameModel.Random.Next(PlanetMinSize, PlanetMaxSize);
            newPlanet.Height = size;
            newPlanet.Width = size;
            newPlanet.Renderer = null;

            return newPlanet;
        }

        public Asteroid AsteroidFactory()
        {
            Asteroid newAsteroid = asteroidPool.GetNew() as Asteroid;

            int size = GameModel.Random.Next(AsteroidMinSize, AsteroidMaxSize);
            newAsteroid.Height = size;
            newAsteroid.Width = size;
            newAsteroid.Stage = AsteroidInitialStage;
            newAsteroid.Style = GameModel.Random.Next(1, 4);
            newAsteroid.Renderer = null;

            return newAsteroid;
        }

        public Laser LaserFactory(Ship ship)
        {
            Laser newLaser = laserPool.GetNew() as Laser;
            if (ship != null)
            {
                newLaser.Owner = ship.Owner;

                newLaser.Location = new Vector2(ship.Location.X, ship.Location.Y);
                newLaser.Height = LaserHeight;
                newLaser.Width = LaserWidth;
                newLaser.Orientation = ship.Orientation;
                newLaser.Velocity = ship.Velocity + thrustVector(LaserSpeed, ship.Orientation);
            }
            newLaser.Renderer = null;

            return newLaser;
        }

        public Entity EntityFactory(EntityType entityType, ObjectState entityState)
        {
            if (entityType == EntityType.Asteroid)
            {
                Asteroid asteroid = AsteroidFactory();
                asteroid.SetState(entityState);
                return asteroid;
            }
            else if (entityType == EntityType.Laser)
            {
                Laser laser = LaserFactory(null);
                laser.SetState(entityState);
                return laser;
            }
            else if (entityType == EntityType.Planet)
            {
                Planet planet = PlanetFactory(new Vector2(0,0), 0);
                planet.SetState(entityState);
                return planet;
            }
            else if (entityType == EntityType.Ship)
            {
                Ship ship = ShipFactory();
                ship.SetState(entityState);
                return ship;
            }
            else
            {
                //log.Warn("Unknown entity type.");
                return null;
            }
        }

        //// TODO: replace this function by creating a delegate in the entity that
        //// the factory methods will set to return the entity to the correct pool.
        public void ReturnToPool(Entity entityToReturn)
        {
            if (entityToReturn is Asteroid)
            {
                asteroidPool.Return(entityToReturn);
            }
            else if (entityToReturn is Laser)
            {
                laserPool.Return(entityToReturn);
            }
            else if (entityToReturn is Planet)
            {
                planetPool.Return(entityToReturn);
            }
            else if (entityToReturn is Ship)
            {
                shipPool.Return(entityToReturn);
            }
        }

        public static Vector2 thrustVector(float thrust, float orientation)
        {
            return new Vector2(thrust * (float)Math.Sin(orientation), -thrust * (float)Math.Cos(orientation));
        }

    }
}
