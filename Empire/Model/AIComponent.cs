using Empire.Network;
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
        private Dictionary<Ship, int> _commandHistory = new Dictionary<Ship, int>();

        internal void Update(Ship player)
        {
            foreach(Ship ship in GameModel.Ships)
            {
                if (ship.Owner == null)
                {
                    issueCommand(ship);
                }
            }
        }

        private void issueCommand(Ship ship)
        {
            // default to 'thrust' command if this ship hasn't done anything yet.
            int currentCommand = (int)CommandFlags.Thrust;

            // retrieve last command
            if ((_commandHistory.ContainsKey(ship)))
            {
                currentCommand = _commandHistory[ship];
                _commandHistory.Remove(ship);
            }

            int changeCommand = GameModel.Random.Next(0, 100);
            if (changeCommand < chanceToSwitchCommands)
            {
                int newCommand = GameModel.Random.Next(0, 5);
                switch (newCommand)
                {
                    case 0:
                        currentCommand = (int)CommandFlags.Thrust;
                        break;
                    case 1:
                        currentCommand = (int)CommandFlags.Right;
                        break;
                    case 2:
                        currentCommand = (int)CommandFlags.Shields;
                        break;
                    case 3:
                        currentCommand = (int)CommandFlags.Left;
                        break;
                    case 4:
                        currentCommand = (int)CommandFlags.Shoot;
                        break;
                }
            }

            // send the command to the ship
            //ship.Command = new ShipCommand(currentCommand);
            _commandHistory.Add(ship, currentCommand);

        }
    }
}
