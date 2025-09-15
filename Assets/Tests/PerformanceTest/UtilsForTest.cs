using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZeroFormatter;
using ZeroFormatter.Formatters;
using ZeroFormatter.Internal;

[System.Serializable]
public class KVPair
{
    public string Key;
    
    public string Value;
    
    public KVPair()
    {
        Key = string.Empty;
        Value = string.Empty;
    }

    public KVPair(string item0, string item1)
    {
        Key = item0;
        Value = item1;
    }

    private static char[] splitCharArr = new[] { '=' };

    public override string ToString()
    {
        return $"{Key}={Value}";
    }
    
    public static KVPair FromString(string str)
    {
        // 这里假设字符串Key中不包含‘=’ "Key=Value"
        var parts = str.Split(splitCharArr, 2);
        if (parts.Length == 2)
        {
            return new KVPair()
            {
                Key = parts[0],
                Value = parts[1]
            };
        }
        else
        {
            return new KVPair()
            {
                Key = str,
                Value = string.Empty
            };
        }
    }
}

public class KVPairFormatter<TTypeResolver> : Formatter<TTypeResolver, global::KVPair>
        where TTypeResolver : ITypeResolver, new()
    {
        readonly Formatter<TTypeResolver, string> formatter0;
        readonly Formatter<TTypeResolver, string> formatter1;
        
        public override bool NoUseDirtyTracker =>
            formatter0.NoUseDirtyTracker
            && formatter1.NoUseDirtyTracker;

        public KVPairFormatter()
        {
            formatter0 = Formatter<TTypeResolver, string>.Default;
            formatter1 = Formatter<TTypeResolver, string>.Default;
            
        }

        public override int? GetLength()
        {
            return formatter0.GetLength() + formatter1.GetLength();
        }

        public override int Serialize(ref byte[] bytes, int offset, global::KVPair value)
        {
            BinaryUtil.EnsureCapacity(ref bytes, offset, 12);
            var startOffset = offset;
            offset += formatter0.Serialize(ref bytes, offset, value.Key);
            offset += formatter1.Serialize(ref bytes, offset, value.Value);
            return offset - startOffset;
        }

        public override global::KVPair Deserialize(ref byte[] bytes, int offset, DirtyTracker tracker, out int byteSize)
        {
            byteSize = 0;
            int size;
            var item0 = formatter0.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            var item1 = formatter1.Deserialize(ref bytes, offset, tracker, out size);
            offset += size;
            byteSize += size;
            
            return new global::KVPair(item0, item1);
        }
    }

public static class UtilsForTest
{
    public static List<KVPair> GenerateTestKvPairListData(int testCount)
    {
        var data = new List<KVPair>(testCount);
        for (int i = 0; i < testCount; i++)
        {
            data.Add(new KVPair { Key = "TEST_KEY_" + i, Value = "TEST_VALUE_" + i });
        }
        return data;
    }
}
