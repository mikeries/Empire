using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmpireUWP.Network
{
    class GameChangedEventArgs : EventArgs
    {
        public GameData GameData;
        
        public GameChangedEventArgs(GameData data)
        {
            GameData = data;
        }
    }
}
