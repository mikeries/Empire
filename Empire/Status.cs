using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire
{
    enum Status
    {
        Active,
        Dead,
        New,
        Disposable,  // this one is used to indicate that the sprite has been removed and it's OK for the model to delete the object.
    }
}
