using ZeroFormatter;

namespace KVSaveSystem
{
    
    [Union(subTypes: new[] { typeof(FloatKvSaveDataObj), typeof(IntKvSaveDataObj) , typeof(StringKvSaveDataObj)}, 
        fallbackType: typeof(StringKvSaveDataObj))]
    public interface ISaveDataObj
    {
        [UnionKey]
        byte TypeKey { get; }
    }
    
    public abstract class KvSaveDataObj<T>
    {
        public virtual T Value { get; set; }
        public bool Equals<T>(T value)
        {
            return Value.Equals(value);
        }
    }
    
    [ZeroFormattable]
    public class FloatKvSaveDataObj : KvSaveDataObj<float>, ISaveDataObj
    {
        [IgnoreFormat] 
        public byte TypeKey => 1;

        [Index(0)]
        public virtual string Key { get; set; }
        [Index(1)]
        public override float Value { get; set; }
    }
    
    [ZeroFormattable]
    public class StringKvSaveDataObj : KvSaveDataObj<string>, ISaveDataObj
    {
        [IgnoreFormat] 
        public byte TypeKey => 2;
        
        [Index(0)]
        public virtual string Key { get; set; }
        [Index(1)]
        public override string Value { get; set; }
    }
    
    [ZeroFormattable]
    public class IntKvSaveDataObj : KvSaveDataObj<int>, ISaveDataObj
    {
        [IgnoreFormat] 
        public byte TypeKey => 3;
        
        [Index(0)]
        public virtual string Key { get; set; }
        [Index(1)]
        public override int Value { get; set; }
    }
}