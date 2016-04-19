using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire.Model
{
    static class ModelHelper
    {
        const int PlanetMinSize = 150;
        const int PlanetMaxSize = 250;
        const int AsteroidMinSize = 100;
        const int AsteroidMaxSize = 200;
        const int AsteroidInitialStage = 2;
        const int LaserHeight = 3;
        const int LaserWidth = 3;

        internal static Entity PlanetFactory(float x, float y, Planets ID)
        {
            Planet newPlanet = new Planet(x, y, ID);

            int size = GameModel.Random.Next(PlanetMinSize, PlanetMaxSize);
            newPlanet.Height = size;
            newPlanet.Width = size;

            return newPlanet;
        }

        internal static Entity AsteroidFactory(float x, float y)
        {
            Asteroid newAsteroid = new Asteroid(x, y);

            int size = GameModel.Random.Next(AsteroidMinSize, AsteroidMaxSize);
            newAsteroid.Height = size;
            newAsteroid.Width = size;
            newAsteroid.Stage = AsteroidInitialStage;
            newAsteroid.Style = GameModel.Random.Next(1, 4);

            return newAsteroid;
        }

        internal static Laser LaserFactory(Player ship)
        {
            Laser laser = new Laser();
            laser.Location = new Vector2(ship.Location.X, ship.Location.Y);
            laser.Height = LaserHeight;
            laser.Width = LaserWidth;
            laser.Orientation = ship.Orientation;
            laser.Velocity = ship.Velocity + thrustVector(15, ship.Orientation);

            return laser;
        }

        internal static Vector2 thrustVector(float thrust, float orientation)
        {
            return new Vector2(thrust * (float)Math.Sin(orientation), -thrust * (float)Math.Cos(orientation));
        }

    }
}
