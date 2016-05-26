using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmpireUWP.Model
{
    // Events fired by the model are simply a property and a value.
    class GameEventArgs : EventArgs
    {
        public int Value { get; private set; }
        public string Property { get; private set; }

        public GameEventArgs(string property, int value)
        {
            Value = value;
            Property = property;
        }
    }
}
