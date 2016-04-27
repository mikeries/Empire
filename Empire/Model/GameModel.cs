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

        // all the game entities that exist
        private static List<Entity> _gameEntities = new List<Entity>();
        public static List<Entity> GameEntities { get { return _gameEntities; } }

        // a list of the ships
        private static List<Ship> _ships = new List<Ship>();
        public static List<Ship> Ships { get { return _ships; } }

        public static Random Random = new Random();
        public static int InitialAsteroidCount = 4000;
        public static int InitialShipCount = 5;

        public GameModel()
        {
        }

        // a basic world initialization.  This needs to be replaced with
        // a class that can generate a new 'world' as well as saving and restoring one.
        public static void Initialize()
        {
            for (int i = 0; i < InitialShipCount; i++) {
                Ship ship = new Ship(View.Game.PlayArea.Width / 2, View.Game.PlayArea.Height / 2);
                Ships.Add(ship);
            }

            for (int i = 0; i < (int)Planets.Count; i++) { 
                ModelHelper.SpawnPlanet(Random.Next(-10000, 10000), Random.Next(-10000, 10000), (Planets)i);
            }

            for (int i = 0; i < InitialAsteroidCount; i++)
            {
                Asteroid asteroid = ModelHelper.SpawnAsteroid(GameModel.Random.Next(View.Game.PlayArea.Left, View.Game.PlayArea.Right), GameModel.Random.Next(View.Game.PlayArea.Left, View.Game.PlayArea.Right));
                asteroid.Velocity = new Vector2(Random.Next(-200, 200)/1000f, Random.Next(-200, 200)/1000f);
            }

        }

        private static bool Collision(Circle c1, Circle c2)
        {
            return ((c1.Center - c2.Center).Length() < (c1.Radius + c2.Radius-1));
        }

        public static void Update(GameTime gameTime)
        {
            // First, update all entities and get rid of those that are done
            foreach (Entity entity in _gameEntities.ToList())
            {
                if (entity.Status == Status.Disposable)
                    _gameEntities.Remove(entity);
                else entity.Update(gameTime);
            }

            // Pull out only the asteroids and check them against the non-asteroids.
            var asteroids =
                from entity in _gameEntities
                where (entity is Asteroid)
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
                            entity.HandleCollision(asteroid);
                            asteroid.HandleCollision(entity);
                        }
                    }
                }
            }

        }

        internal static Ship GetShip(string connectionID)
        {
            Ship ship = Ships.Find(
                delegate (Ship s)
                {
                    return (s.Owner == connectionID);
                }
                );


            if (ship != null) return ship;  // early exit

            foreach (Ship s in Ships)
            {
                if (s.Owner == null)
                {
                    s.Owner = connectionID;
                    return s;
                }
            }
            

            //TODO: Log this and throw exception
            return null;
        }

        //public static event EventHandler<GameEventArgs> GameChanged = delegate { };
        //private static void OnGameChanged(string property, int value)
        //{
        //        GameChanged(null, new GameEventArgs(property,value));
        //}
    }
}
