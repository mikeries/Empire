using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire.Model
{
    // very crude artificial intelligence to put some basic life into
    // the non-PC ships.

    // TODO: target selection, pathfinding, etc.
    public class AIComponent
    {
        private const int chanceToSwitchCommands = 1;  // percent chance that the ship picks a new command
        private Dictionary<Ship, KeyboardState> _commandHistory = new Dictionary<Ship, KeyboardState>();

        internal void Update(Ship player)
        {
            foreach(Ship ship in GameModel.Ships)
            {
               // if (ship != player)
                {
                    issueCommand(ship);
                }
            }
        }

        private void issueCommand(Ship ship)
        {
            // default to 'thrust' command if this ship hasn't done anything yet.
            KeyboardState keyboardState = new KeyboardState(Keys.Up);

            // retrieve last command
            if ((_commandHistory.ContainsKey(ship)))
            {
                keyboardState = _commandHistory[ship];
                _commandHistory.Remove(ship);
            }

            int changeCommand = GameModel.Random.Next(0, 100);
            if (changeCommand < chanceToSwitchCommands)
            {
                int newCommand = GameModel.Random.Next(0, 5);
                switch (newCommand)
                {
                    case 0:
                        keyboardState = new KeyboardState(Keys.Up);
                        break;
                    case 1:
                        keyboardState = new KeyboardState(Keys.Right);
                        break;
                    case 2:
                        keyboardState = new KeyboardState(Keys.Down);
                        break;
                    case 3:
                        keyboardState = new KeyboardState(Keys.Left);
                        break;
                    case 4:
                        keyboardState = new KeyboardState(Keys.Space);
                        break;
                }
            }

            // send the command to the ship
            ship.Command = new ShipCommand(keyboardState);
            _commandHistory.Add(ship, keyboardState);

        }
    }
}
