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
        public static Entity PlanetFactory(int x, int y, Planets ID)
        {
            Planet newPlanet = new Planet(x, y, ID);

            int size = GameModel.Random.Next(150, 250);
            newPlanet.Height = size;
            newPlanet.Width = size;
            return newPlanet;
        }

        public static Entity AsteroidFactory(int x, int y)
        {
            Asteroid asteroid = new Asteroid(x, y);

            int size = GameModel.Random.Next(100, 200);
            asteroid.Height = size;
            asteroid.Width = size;
            asteroid.Stage = 2;
            asteroid.Style = GameModel.Random.Next(1, 4);
            return asteroid;
        }
        
    }
}
