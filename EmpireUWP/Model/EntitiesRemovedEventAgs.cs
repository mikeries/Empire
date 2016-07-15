using System.Collections.Generic;

namespace EmpireUWP.Model
{
    internal class EntitiesRemovedEventAgs
    {
        internal List<Entity> EntitiesRemoved;

        internal EntitiesRemovedEventAgs(List<Entity> deadEntities)
        {
            EntitiesRemoved = deadEntities;
        }
    }
}