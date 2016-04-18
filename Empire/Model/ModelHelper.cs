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

        public static Entity PlanetFactory(int x, int y, Planets ID)
        {
            Planet newPlanet = new Planet(x, y, ID);

            int size = GameModel.Random.Next(PlanetMinSize, PlanetMaxSize);
            newPlanet.Height = size;
            newPlanet.Width = size;

            return newPlanet;
        }

        public static Entity AsteroidFactory(int x, int y)
        {
            Asteroid newAsteroid = new Asteroid(x, y);

            int size = GameModel.Random.Next(AsteroidMinSize, AsteroidMaxSize);
            newAsteroid.Height = size;
            newAsteroid.Width = size;
            newAsteroid.Stage = AsteroidInitialStage;
            newAsteroid.Style = GameModel.Random.Next(1, 4);

            return newAsteroid;
        }
        
    }
}
