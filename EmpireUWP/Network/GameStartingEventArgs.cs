using System;
using System.Collections.Generic;

namespace EmpireUWP.Network
{
    internal class GameStartingEventArgs : EventArgs
    {
        private GameData gameData;
        private Dictionary<string, PlayerData> players;

        public GameStartingEventArgs(Dictionary<string, PlayerData> players, GameData gameData)
        {
            this.players = players;
            this.gameData = gameData;
        }
    }
}