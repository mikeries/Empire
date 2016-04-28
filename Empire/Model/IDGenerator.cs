using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire.Model
{
    static class IDGenerator
    {
        private static int ID = 0;

        static IDGenerator()
        {

        }

        public static int NewID()
        {
            ID = ID + 1;
            return ID;
        }
    }
}
