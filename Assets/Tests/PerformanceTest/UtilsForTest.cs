using System;
using System.Collections.Generic;
using MemoryPack;
using Nino.Core;
using ZeroFormatter;

[MemoryPackable]
[ZeroFormattable]
[Serializable]
[NinoType]
public partial class KVPair
{
    [Index(0)] public virtual string Key { get; set; }

    [Index(1)] public virtual string Value { get; set; }

    [MemoryPackConstructor]
    public KVPair(string key, string value)
    {
        Key = key;
        Value = value;
    }
}

public static class UtilsForTest
{
    public static List<KVPair> GenerateTestKvPairListData(int testCount)
    {
        var data = new List<KVPair>(testCount);
        for (int i = 0; i < testCount; i++)
        {
            data.Add(new KVPair("TEST_KEY_" + i, "TEST_VALUE_" + i));
        }

        return data;
    }
}