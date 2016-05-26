using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmpireUWP
{
    // Each entity has a Status variable that is used to identity its state
    // Typically, the lifecycle is New->Alive->Dead->Disposable
    enum Status
    {
        Active,     // the entity is 'alive' and active
        Dead,       // the entity is 'dead' and will be removed the the world.  When the sprite is removed, the Status becomes Disposable
        New,        // the entity has been spawned, but no sprite exists yet.  When the sprite has been created, the Status becomes Alive
        Disposable, // this one is used to indicate that the sprite has been removed and it's OK for the model to delete the object.
        Count,
    }
}
