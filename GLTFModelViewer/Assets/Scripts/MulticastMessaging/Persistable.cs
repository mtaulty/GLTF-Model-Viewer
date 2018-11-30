using System.IO;
using System.Runtime.Serialization;

namespace MulticastMessaging
{    
    public abstract class Persistable
    {
        public abstract void Save(BinaryWriter writer);
        public abstract void Load(BinaryReader reader);
    }
}
