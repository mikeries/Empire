using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Empire
{
    
    // ObjectStates can be created by calling GetState on an entity object and can
    // be used to initialize an object to this state by calling SetState on an Entity.
    // This enables us to transfer an object state over the network and transfer it to 
    // an unused object in the object pool on the receiving machine, rather than creating a new Entity object.

    // A faster implementation might be to simply use a byte[] array, but this would be harder to maintain
    // as it would require the calling objects to be careful about keeping the SetData and GetData functions
    // parallel.  There could also be issues if the sender and receiver are using different revisions.

    [Serializable]
    class ObjectState : ISerializable
    {
        public Dictionary<string,byte[]> Data = new Dictionary<string, byte[]>();

        internal ObjectState() { }

        public ObjectState(SerializationInfo info, StreamingContext context)
        {
            IFormatterConverter converter = new FormatterConverter();

            foreach(SerializationEntry entry in info)
            {
                Data[entry.Name] = (byte[])converter.Convert(entry.Value, entry.ObjectType);
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            foreach (String key in Data.Keys)
            {
                info.AddValue(key, Data[key]);
            }
        }

        internal void AddValue(string key, int value)
        {
            Data.Add(key, BitConverter.GetBytes(value));
        }

        internal int GetInt(string key)
        {
            return BitConverter.ToInt32(Data[key],0);
        }

        internal void AddValue(string key, float value)
        {
            Data.Add(key, BitConverter.GetBytes(value));
        }

        internal float GetFloat(string key)
        {
            return BitConverter.ToSingle(Data[key], 0);
        }

        internal void AddValue(string key, string value)
        {
            Data.Add(key, Encoding.ASCII.GetBytes(value));
        }

        internal string GetString(string key)
        {
            return Encoding.ASCII.GetString(Data[key]);
        }

    }
}
