using Nino.Core;

namespace KVSaveSystem
{
    [NinoType]
    //TODO [XLua.BlackList]
    public interface ISaveDataObj
    {

    }
    
    [NinoType]
    //TODO [XLua.BlackList]
    public class KvSaveDataObj<T> : ISaveDataObj
    {
        public T Value;
        public bool Equals<T>(T value)
        {
            return Value.Equals(value);
        }
    }
}