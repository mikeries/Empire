using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EmpireUWP
{
    
    // ObjectStates can be created by calling GetState on an entity object and can
    // be used to initialize an object to this state by calling SetState on an Entity.
    // This enables us to transfer an object state over the network and transfer it to 
    // an unused object in the object pool on the receiving machine, rather than creating a new Entity object.

    // A faster implementation might be to simply use a byte[] array, but this would be harder to maintain
    // as it would require the calling objects to be careful about keeping the SetData and GetData functions
    // parallel.  There could also be issues if the sender and receiver are using different revisions.

    [DataContract (Namespace="http://schemas.datacontract.org/2004/07/EmpireSerializer")]
    public class ObjectState
    {
        [DataMember]
        public Dictionary<string,byte[]> Data = new Dictionary<string, byte[]>();

        internal ObjectState() { }

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
