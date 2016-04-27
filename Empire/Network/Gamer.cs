using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Empire.Network
{
    public class Gamer
    {
        public string Name { get; set; }
        public string ConnectionID { get; private set; }                   // the ID corresponding to this gamer
        public IPEndPoint EndPoint { get; private set; }

        public Gamer(string name, IPEndPoint endpoint)
        {
            Name = name;
            EndPoint = endpoint;
            ConnectionID = EndPoint.ToString();
        }
    }
}
