using Nino.Core;

namespace KVSaveSystem
{
    [NinoType]
    public interface ISaveDataObj
    {

    }
    
    [NinoType]
    public class KvSaveDataObj<T> : ISaveDataObj
    {
        public T Value;
        public bool Equals<T>(T value)
        {
            return Value.Equals(value);
        }
    }
}