using System.Collections.Generic;

namespace EmpireUWP.Model
{
    public class EntitiesRemovedEventAgs
    {
        public List<Entity> EntitiesRemoved;

        public EntitiesRemovedEventAgs(List<Entity> deadEntities)
        {
            EntitiesRemoved = deadEntities;
        }
    }
}