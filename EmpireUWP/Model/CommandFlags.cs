using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmpireUWP.Model
{
    public enum CommandFlags : int
    {
        Thrust = 2,
        Shields = 4,
        Shoot = 8,
        Right = 16,
        Left = 32,
    }
}
