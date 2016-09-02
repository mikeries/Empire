using System.Collections.Generic;

namespace EmpireUWP.Model
{
    public class EntitiesRemovedEventAgs
    {
        public List<int> EntitiesRemoved;

        public EntitiesRemovedEventAgs(List<int> deadEntities)
        {
            EntitiesRemoved = deadEntities;
        }
    }
}