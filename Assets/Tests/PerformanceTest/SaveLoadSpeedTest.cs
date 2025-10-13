using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class SaveLoadSpeedTest : MonoBehaviour
{
    [LabelText("重复次数(多数情况无用)")] 
    public int repeatTimes = 1;
    
    private const string TEST_SAVE = "测试保存";

    [TitleGroup(TEST_SAVE)] 
    [Button("测试 PlayerPrefs 保存")]
    public void TestPlayerPrefsSave()
    {
        List<long> times = new List<long>(repeatTimes);
        for (int i = 0; i < repeatTimes; i++)
        {
            Stopwatch sw = Stopwatch.StartNew();
            PlayerPrefs.Save();
            sw.Stop();
            times.Add(sw.ElapsedMilliseconds);
        }

        UnityEngine.Debug.Log($"PlayerPrefs Save: {times.Average()} ms");
    }

    // [TitleGroup(TEST_SAVE)] 
    // [Button("测试 FBPP 保存")]
    // public void TestFBPPSave()
    // {
    //
    // }


    [TitleGroup(TEST_SAVE)] 
    [Button("测试 EasySave 保存(缓存优化)")]
    public void TestEasySave()
    {
        List<long> times = new List<long>(repeatTimes);
        ES3Settings es3CacheSetting = new ES3Settings();
        es3CacheSetting.location = ES3.Location.Cache;

        for (int i = 0; i < repeatTimes; i++)
        {
            Stopwatch sw = Stopwatch.StartNew();
            ES3.StoreCachedFile();
            sw.Stop();
            times.Add(sw.ElapsedMilliseconds);
        }

        UnityEngine.Debug.Log($"KVSaveSystem Set: {times.Average()} ms");
    }
    
    [TitleGroup(TEST_SAVE)]
    [Button("测试 KVSaveSystem 保存")]
    public void TestSaveSystemSave()
    {
        List<long> times = new List<long>(repeatTimes);
        for (int i = 0; i < repeatTimes; i++)
        {
            Stopwatch sw = Stopwatch.StartNew();
            KVSaveSystem.KvSaveSystem.SaveAsync();
            sw.Stop();
            times.Add(sw.ElapsedMilliseconds);
        }

        UnityEngine.Debug.Log($"KVSaveSystem Save: {times.Average()} ms");
    }
    
    
    private const string TEST_LOAD = "测试加载";
    
    // [TitleGroup(TEST_LOAD)]
    // [Button("测试 EasySave 加载(缓存优化)")]
    // public void TestEasyLoad()
    // {
    //     List<long> times = new List<long>(repeatTimes);
    //     ES3Settings es3CacheSetting = new ES3Settings();
    //     es3CacheSetting.location = ES3.Location.Cache;
    //
    //     for (int i = 0; i < repeatTimes; i++)
    //     {
    //         Stopwatch sw = Stopwatch.StartNew();
    //         ES3.Load();
    //         sw.Stop();
    //         times.Add(sw.ElapsedMilliseconds);
    //     }
    //
    //     UnityEngine.Debug.Log($"KVSaveSystem Get: {times.Average()} ms");
    // }
    
    [TitleGroup(TEST_LOAD)]
    [Button("测试 KVSaveSystem 加载")]
    public void TestSaveSystemLoad()
    {
        List<long> times = new List<long>(repeatTimes);
        for (int i = 0; i < repeatTimes; i++)
        {
            Stopwatch sw = Stopwatch.StartNew();
            KVSaveSystem.KvSaveSystem.LoadAllAsync(SaveConfig.PublicArchiveDirectoryPath);
            sw.Stop();
            times.Add(sw.ElapsedMilliseconds);
        }

        UnityEngine.Debug.Log($"KVSaveSystem Load: {times.Average()} ms");
    }
}
