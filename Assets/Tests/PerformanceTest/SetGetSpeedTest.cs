using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class SetGetSpeedTest : MonoBehaviour
{
    [LabelText("测试数据量")]
    public int testCount = 10000;
    
    [LabelText("重复次数")] 
    public int repeatTimes = 1;

    private const string TEST_SET = "测试设置";

    [TitleGroup(TEST_SET)] 
    [Button("测试 PlayerPrefs 设置值")]
    public void TestPlayerPrefsSet()
    {
        var data = UtilsForTest.GenerateTestKvPairListData(testCount);
        List<long> times = new List<long>(repeatTimes);
        for (int i = 0; i < repeatTimes; i++)
        {
            Stopwatch sw = Stopwatch.StartNew();
            for (int j = 0; j < testCount; j++)
            {
                var kv = data[j];
                PlayerPrefs.SetString(kv.Key, kv.Value);
            }

            sw.Stop();
            times.Add(sw.ElapsedMilliseconds);
        }

        UnityEngine.Debug.Log($"PlayerPrefs Set: {times.Average()} ms with {testCount} items each.");
    }

    [TitleGroup(TEST_SET)] 
    [Button("测试 FBPP 设置值")]
    public void TestFBPPSet()
    {
        var data = UtilsForTest.GenerateTestKvPairListData(testCount);
        List<long> times = new List<long>(repeatTimes);
        FBPP.Start(new FBPPConfig());

        for (int i = 0; i < repeatTimes; i++)
        {
            Stopwatch sw = Stopwatch.StartNew();
            for (int j = 0; j < testCount; j++)
            {
                var kv = data[j];
                FBPP.SetString(kv.Key, kv.Value);
            }

            sw.Stop();
            times.Add(sw.ElapsedMilliseconds);
        }

        UnityEngine.Debug.Log($"FBPP Set: {times.Average()} ms with {testCount} items each.");
    }


    [TitleGroup(TEST_SET)] 
    [Button("测试 EasySave 设置值(缓存优化)")]
    public void TestEasySaveSet()
    {
        var data = UtilsForTest.GenerateTestKvPairListData(testCount);
        List<long> times = new List<long>(repeatTimes);
        ES3Settings es3CacheSetting = new ES3Settings();
        es3CacheSetting.location = ES3.Location.Cache;

        for (int i = 0; i < repeatTimes; i++)
        {
            Stopwatch sw = Stopwatch.StartNew();
            for (int j = 0; j < testCount; j++)
            {
                var kv = data[j];
                ES3.Save(kv.Key, kv.Value, es3CacheSetting);
            }

            //ES3.StoreCachedFile();
            sw.Stop();
            times.Add(sw.ElapsedMilliseconds);
        }

        UnityEngine.Debug.Log($"SaveSystem Set: {times.Average()} ms with {testCount} items each.");
    }

    [TitleGroup(TEST_SET)] 
    [Button("测试 KVSaveSystem 设置值")]
    public void TestSaveSystemSet()
    {
        var data = UtilsForTest.GenerateTestKvPairListData(testCount);
        List<long> times = new List<long>(repeatTimes);

        for (int i = 0; i < repeatTimes; i++)
        {
            Stopwatch sw = Stopwatch.StartNew();
            for (int j = 0; j < testCount; j++)
            {
                var kv = data[j];
                KvSaveSystem.SetString(kv.Key, kv.Value);
            }

            sw.Stop();
            times.Add(sw.ElapsedMilliseconds);
        }

        UnityEngine.Debug.Log($"KVSaveSystem Set: {times.Average()} ms with {testCount} items each.");
    }
    
    
    private const string TEST_GET = "测试获取";

    [TitleGroup(TEST_GET)] 
    [Button("测试 PlayerPrefs 获取值")]
    public void TestPlayerPrefsGet()
    {
        var data = UtilsForTest.GenerateTestKvPairListData(testCount);
        List<long> times = new List<long>(repeatTimes);
        for (int i = 0; i < repeatTimes; i++)
        {
            Stopwatch sw = Stopwatch.StartNew();
            for (int j = 0; j < testCount; j++)
            {
                var kv = data[j];
                PlayerPrefs.GetString(kv.Key, kv.Value);
            }

            sw.Stop();
            times.Add(sw.ElapsedMilliseconds);
        }

        UnityEngine.Debug.Log($"PlayerPrefs Get: {times.Average()} ms with {testCount} items each.");
    }
    
    [TitleGroup(TEST_GET)] 
    [Button("测试 FBPP 获取值")]
    public void TestFBPPGet()
    {
        var data = UtilsForTest.GenerateTestKvPairListData(testCount);
        List<long> times = new List<long>(repeatTimes);
        FBPP.Start(new FBPPConfig());

        for (int i = 0; i < repeatTimes; i++)
        {
            Stopwatch sw = Stopwatch.StartNew();
            for (int j = 0; j < testCount; j++)
            {
                var kv = data[j];
                FBPP.GetString(kv.Key, kv.Value);
            }

            sw.Stop();
            times.Add(sw.ElapsedMilliseconds);
        }

        UnityEngine.Debug.Log($"FBPP Get: {times.Average()} ms with {testCount} items each.");
    }

    [TitleGroup(TEST_GET)] 
    [Button("测试 EasySave 获取值(缓存优化)")]
    public void TestEasySaveGet()
    {
        var data = UtilsForTest.GenerateTestKvPairListData(testCount);
        List<long> times = new List<long>(repeatTimes);
        ES3Settings es3CacheSetting = new ES3Settings();
        es3CacheSetting.location = ES3.Location.Cache;
        object defaultValue = "";

        for (int i = 0; i < repeatTimes; i++)
        {
            Stopwatch sw = Stopwatch.StartNew();
            for (int j = 0; j < testCount; j++)
            {
                var kv = data[j];
                ES3.Load(kv.Key, defaultValue, es3CacheSetting);
            }

            sw.Stop();
            times.Add(sw.ElapsedMilliseconds);
        }

        UnityEngine.Debug.Log($"KVSaveSystem Get: {times.Average()} ms with {testCount} items each.");
    }

    [TitleGroup(TEST_GET)] 
    [Button("测试 KVSaveSystem 获取值")]
    public void TestSaveSystemGet()
    {
        var data = UtilsForTest.GenerateTestKvPairListData(testCount);
        List<long> times = new List<long>(repeatTimes);

        for (int i = 0; i < repeatTimes; i++)
        {
            Stopwatch sw = Stopwatch.StartNew();
            for (int j = 0; j < testCount; j++)
            {
                var kv = data[j];
                KvSaveSystem.GetString(kv.Key, kv.Value);
            }

            sw.Stop();
            times.Add(sw.ElapsedMilliseconds);
        }

        UnityEngine.Debug.Log($"KVSaveSystem Get: {times.Average()} ms with {testCount} items each.");
    }
}