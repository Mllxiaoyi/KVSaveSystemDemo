using System.Collections;
using System.Collections.Generic;
using System.IO;
using KVSaveSystem;
using RuntimeUnitTestToolkit;
using Sirenix.OdinInspector;
using UnityEngine;

public class KvSaveSystemSample : MonoBehaviour
{
    string sampleGroupName = "SampleGroup";

    [Button("Save Sample Data")]
    public void SaveSampleData()
    {
        KvSaveSystem.SetInt("FirstEnter", 1, "SampleGroup");
    }

    [Button("Load Sample Data")]
    public void LoadSampleData()
    {
        KvSaveSystem.LoadAll();
        var value = KvSaveSystem.GetInt("FirstEnter", 0, "SampleGroup");
        Debug.Log($"FirstEnter: {value}");
    }

    [Button("Save Multi Data")]
    public void SaveMultiData()
    {
        KvSaveSystem.SetInt("FirstEnter", 1, sampleGroupName);
        KvSaveSystem.SetInt("TotalDamage", 2124657963, sampleGroupName);
        KvSaveSystem.SetFloat("HpRate", 3.3f, sampleGroupName);
        KvSaveSystem.SetString("PlayerName", "玩家名称", sampleGroupName);
        KvSaveSystem.SetString("ChangeLine1", "测试\r\n换行\t1", sampleGroupName);
        KvSaveSystem.SetString("ChangeLine2", @"测试
换行2", sampleGroupName);
        KvSaveSystem.Save(true);
    }

    [Button("Load Multi Data")]
    public void LoadMultiData()
    {
        KvSaveSystem.LoadAll();
    }
}