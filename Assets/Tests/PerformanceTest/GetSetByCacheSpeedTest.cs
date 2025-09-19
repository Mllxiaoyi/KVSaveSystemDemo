using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

public class GetSetByCacheSpeedTest : MonoBehaviour
{

    Dictionary<string, KVPair> dict = new Dictionary<string, KVPair>();

    [Button("测试简单字典读取")]
    public void TestSimpleDict(int testCount, int repeatTimes)
    {
        var list = UtilsForTest.GenerateTestKvPairListData(testCount);
        dict = list.ToDictionary(kv => kv.Key, kv => kv);
        List<string> keyList = new List<string>(testCount);
        keyList.AddRange(list.Select(kv => kv.Key));

        List<long> writeTimes = new List<long>(repeatTimes);
        List<long> setTimes = new List<long>(repeatTimes);
        for (int i = 0; i < repeatTimes; i++)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            for (int j = 0; j < testCount; j++)
            {
                var value = dict[keyList[j]];
            }

            sw.Stop();
            writeTimes.Add(sw.ElapsedMilliseconds);

            sw = System.Diagnostics.Stopwatch.StartNew();
            for (int j = 0; j < testCount; j++)
            {
                dict[keyList[j]] = list[j];
            }

            sw.Stop();
            setTimes.Add(sw.ElapsedMilliseconds);
        }

        Debug.Log($"原始数据从字典读取: {writeTimes.Average()} ms, 从设置到字典中：{setTimes.Average()} ms");
    }

    Dictionary<string, KVPair> zeroFormatterDict = new Dictionary<string, KVPair>();

    [Button("测试 ZeroFormatter 字典读取")]
    public void TestZeroFormatterDict(int testCount, int repeatTimes)
    {
        IList<KVPair> list = UtilsForTest.GenerateTestKvPairListData(testCount);
        byte[] bytes = ZeroFormatter.ZeroFormatterSerializer.Serialize(list);
        IList<KVPair> deserializedList = ZeroFormatter.ZeroFormatterSerializer.Deserialize<IList<KVPair>>(bytes);
        zeroFormatterDict = deserializedList.ToDictionary(kv => kv.Key, kv => kv);
        List<string> keyList = new List<string>(testCount);
        keyList.AddRange(deserializedList.Select(kv => kv.Key));

        List<long> writeTimes = new List<long>(repeatTimes);
        List<long> setTimes = new List<long>(repeatTimes);
        for (int i = 0; i < repeatTimes; i++)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            for (int j = 0; j < testCount; j++)
            {
                var value = zeroFormatterDict[keyList[j]];
            }

            sw.Stop();
            writeTimes.Add(sw.ElapsedMilliseconds);

            sw = System.Diagnostics.Stopwatch.StartNew();
            for (int j = 0; j < testCount; j++)
            {
                zeroFormatterDict[keyList[j]] = deserializedList[j];
            }

            sw.Stop();
            setTimes.Add(sw.ElapsedMilliseconds);
        }

        Debug.Log($"ZeroFormatter 序列化数据从字典读取: {writeTimes.Average()} ms, 从设置到字典中：{setTimes.Average()} ms");
    }
    
    Dictionary<string, KVPair> memoryPackDict = new Dictionary<string, KVPair>();
    [Button("测试 MemoryPack 字典读取")]
    public void TestMemoryPackDict(int testCount, int repeatTimes)
    {
        IList<KVPair> list = UtilsForTest.GenerateTestKvPairListData(testCount);
        byte[] bytes = MemoryPack.MemoryPackSerializer.Serialize(list);
        IList<KVPair> deserializedList = MemoryPack.MemoryPackSerializer.Deserialize<IList<KVPair>>(bytes);
        memoryPackDict = deserializedList.ToDictionary(kv => kv.Key, kv => kv);
        List<string> keyList = new List<string>(testCount);
        keyList.AddRange(deserializedList.Select(kv => kv.Key));

        List<long> writeTimes = new List<long>(repeatTimes);
        List<long> setTimes = new List<long>(repeatTimes);
        for (int i = 0; i < repeatTimes; i++)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            for (int j = 0; j < testCount; j++)
            {
                var value = memoryPackDict[keyList[j]];
            }

            sw.Stop();
            writeTimes.Add(sw.ElapsedMilliseconds);

            sw = System.Diagnostics.Stopwatch.StartNew();
            for (int j = 0; j < testCount; j++)
            {
                memoryPackDict[keyList[j]] = deserializedList[j];
            }

            sw.Stop();
            setTimes.Add(sw.ElapsedMilliseconds);
        }

        Debug.Log($"MemoryPack 序列化数据从字典读取: {writeTimes.Average()} ms, 从设置到字典中：{setTimes.Average()} ms");
    }

    Dictionary<string, KVPair> ninoDict = new Dictionary<string, KVPair>();

    [Button("测试 NinoSerializer 字典读取")]
    public void TestNinoSerializerDict(int testCount, int repeatTimes)
    {
        IList<KVPair> list = UtilsForTest.GenerateTestKvPairListData(testCount);
        byte[] bytes = NinoSerializer.Serialize(list);
        IList<KVPair> deserializedList = NinoDeserializer.Deserialize<IList<KVPair>>(bytes);
        ninoDict = deserializedList.ToDictionary(kv => kv.Key, kv => kv);
        List<string> keyList = new List<string>(testCount);
        keyList.AddRange(deserializedList.Select(kv => kv.Key));

        List<long> writeTimes = new List<long>(repeatTimes);
        List<long> setTimes = new List<long>(repeatTimes);
        for (int i = 0; i < repeatTimes; i++)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            for (int j = 0; j < testCount; j++)
            {
                var value = ninoDict[keyList[j]];
            }

            sw.Stop();
            writeTimes.Add(sw.ElapsedMilliseconds);

            sw = System.Diagnostics.Stopwatch.StartNew();
            for (int j = 0; j < testCount; j++)
            {
                ninoDict[keyList[j]] = deserializedList[j];
            }

            sw.Stop();
            setTimes.Add(sw.ElapsedMilliseconds);
        }

        Debug.Log($"NinoSerializer 序列化数据从字典读取: {writeTimes.Average()} ms, 从设置到字典中：{setTimes.Average()} ms");
    }
}