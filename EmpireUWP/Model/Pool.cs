using EmpireUWP.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//TODO:  Clean up the abstraction here.  Maybe implement an IPoolable interface and make this class
// not be specific to Entities?
namespace EmpireUWP
{
    // Class to implement a pool of entities, allowing object reuse to avoid heap fragmentation.
    public class EntityPool<T>
    {
        private Entity[] _objects;
        private Func<T> _objectGenerator;
        private int nextNew;
        private int _maxSize;
        private static object _accessLock;

        public EntityPool(Func<T> objectGenerator, int maxSize)
        {
            _maxSize = maxSize;
            _objects = new Entity[_maxSize];
            _objectGenerator = objectGenerator;

            for (int i = 0; i < maxSize; i++)
            {
                Entity item = _objectGenerator() as Entity;
                item.Status = Status.Disposable;
                item.Initialize();
                _objects[i] = item;
            }
            nextNew = 0;
            _accessLock = new object();
        }

        public Entity GetNew()
        {
            lock (_accessLock)
            {
                Entity item = _objects[nextNew];
                if (item.Status != Status.Disposable)
                {
                    //log.Fatal("Attempted to exceed maximum number of objects in the pool.  Increase the pool size.");
                    throw new OverflowException("Attempted to exceed maximum number of objects in the pool.  Increase the pool size.");
                }
                
                item.Initialize();
                item.Status = Status.Active;

                nextNew = nextNew + 1;
                if (nextNew == _maxSize) nextNew = 0;
                return item;
            }
        }

        public void Return(Entity itemToReturn)
        {
            lock (_accessLock) {
                int index = Array.IndexOf(_objects, itemToReturn);

                if (index < 0)
                {
                    //log.Fatal("Attempted to return a non-pool object to the pool. Type:" + itemToReturn.Type);
                    throw new OverflowException("Attempted to return a non-pool object to the pool. Type:" + itemToReturn.Type);
                }

                Entity item = _objects[index];

                if (index >= nextNew)
                {
                    //log.Warn("Attempted to return an item that was already available.");
                    return;
                }

                item.Status = Status.Disposable;

                // now swap the disposable item with the last active item in the array,
                // which is located at nextNew - 1
                nextNew = nextNew - 1;
                Entity swapItem = _objects[nextNew];
                _objects[nextNew] = _objects[index];
                _objects[index] = swapItem;
            }
        }

    }
}
