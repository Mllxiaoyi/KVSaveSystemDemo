using System.Collections;
using System.Collections.Generic;
using KVSaveSystem;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class KvSaveSystemSample : MonoBehaviour
{
    string sampleGroupName = "SampleGroup";
    
    private void Start()
    {
        DoTestSerialize();
    }

    // 测试 Nino 序列化接口（别删）
    private void DoTestSerialize()
    {
        Dictionary<string, ISaveDataObj> dic = new Dictionary<string, ISaveDataObj>();
        dic.Add("11", new KvSaveDataObj<int>(){Value = 1});
        dic.Add("12", new KvSaveDataObj<float>(){Value = 1.1f});
        dic.Add("13", new KvSaveDataObj<string>(){Value = "11"});
        
        var bytes = NinoSerializer.Serialize(dic);
        dic = NinoDeserializer.Deserialize<Dictionary<string, ISaveDataObj>>(bytes);
    }
    

    [Button("Save Sample Data")]
    public void SaveSampleData()
    {
        KvSaveSystem.SetInt("FirstEnter", 1, "SampleGroup");
    }

    [Button("Load Sample Data")]
    public void LoadSampleData()
    {
        KvSaveSystem.LoadAll(SaveSystemConst.PublicArchiveDirectoryPath);
        var value = KvSaveSystem.GetInt("FirstEnter", 0, "SampleGroup");
        Debug.Log($"FirstEnter: {value}");
    }

    [Button("Save Multi Data"), ButtonGroup("SampleGroup")]
    public void SaveMultiData()
    {
        KvSaveSystem.SetInt("FirstEnter", 1, sampleGroupName);
        KvSaveSystem.SetInt("TotalDamage", 2124657963, sampleGroupName);
        KvSaveSystem.SetFloat("HpRate", 3.3f, sampleGroupName);
        KvSaveSystem.SetString("PlayerName", "玩家名称", sampleGroupName);
        KvSaveSystem.SetString("ChangeLine1", "测试\r\n换行\t1", sampleGroupName);
        KvSaveSystem.SetString("ChangeLine2", @"测试
换行2", sampleGroupName);
        KvSaveSystem.SaveAsyncInternal(true);
    }
    

    [Button("Save MultiTimes In Single Frame")]
    public void SaveMultiTimesInSingleFrame()
    {
        StartCoroutine(SaveMultiTimesInSingleFrameCo());
    }

    public IEnumerator SaveMultiTimesInSingleFrameCo()
    {
        var repeatTimes = 10000;
        for (int i = 0; i < repeatTimes; i++)
        {
            KvSaveSystem.SetInt($"TotalDamage{i}", i, sampleGroupName);
        }

        KvSaveSystem.SetFloat("HpRate", 6.3f, sampleGroupName);
        KvSaveSystem.SetString("PlayerName", "玩家名称", sampleGroupName);
        KvSaveSystem.SaveAsyncInternal();

        for (int i = 0; i < repeatTimes; i++)
        {
            KvSaveSystem.SetInt($"TotalDamage{i}", i + repeatTimes, sampleGroupName);
        }

        KvSaveSystem.SetFloat("HpRate", 6.3f, sampleGroupName);
        KvSaveSystem.SetString("NickName", "玩家昵称", sampleGroupName);
        KvSaveSystem.SaveAsyncInternal();

        for (int i = 0; i < repeatTimes; i++)
        {
            KvSaveSystem.SetInt($"TotalDamage{i}", i + 2 * repeatTimes, sampleGroupName);
        }

        KvSaveSystem.SetString("UserName", "用户名称", sampleGroupName);
        KvSaveSystem.SaveAsyncInternal();
        
        yield return null;
    }
    



    [Button("Test Concurrent Safety")]
    public void TestConcurrentSafety()
    {
        StartCoroutine(ConcurrentSafetyTest());
    }

    /// <summary>
    /// 并发安全性测试 - 验证修改后的系统是否能正确处理并发保存
    /// </summary>
    private IEnumerator ConcurrentSafetyTest()
    {
        Debug.Log("=== 🧪 开始并发安全性测试 ===");

        // 模拟高频并发保存场景
        for (int round = 0; round < 3; round++)
        {
            Debug.Log($"--- 📋 测试轮次 {round + 1} ---");

            // 在同一帧内快速执行多次保存操作（模拟SaveMultiTimesInSingleFrame的问题场景）
            for (int i = 0; i < 3; i++)
            {
                KvSaveSystem.SetInt("TestCounter", round * 100 + i, sampleGroupName);
                KvSaveSystem.SetString("TestMessage", $"Round{round}_Save{i}", sampleGroupName);
                KvSaveSystem.SaveAsyncInternal();
                Debug.Log($"📤 发起保存请求 {i + 1}：Counter={round * 100 + i}");
            }

            // 等待这一轮保存完成
            yield return new WaitForSeconds(1.5f);
        }

        Debug.Log("⏳ 等待所有保存操作完成...");
        yield return new WaitForSeconds(2.0f);

        // 重新加载验证最终数据
        Debug.Log("📖 重新加载数据进行验证...");
        KvSaveSystem.LoadAll(SaveSystemConst.PublicArchiveDirectoryPath);

        var finalCounter = KvSaveSystem.GetInt("TestCounter", -1, sampleGroupName);
        var finalMessage = KvSaveSystem.GetString("TestMessage", "NOT_FOUND", sampleGroupName);

        Debug.Log($"📊 最终数据验证 - Counter: {finalCounter}, Message: {finalMessage}");

        // 验证数据一致性：最终数据应该是最后一次保存的值（Round2_Save2 = 202）
        if (finalCounter == 202 && finalMessage == "Round2_Save2")
        {
            Debug.Log("✅ 并发安全性测试 PASSED - 数据一致性正确！");
            Debug.Log("🎉 系统成功避免了文件访问冲突，只保存了最新数据");
        }
        else
        {
            Debug.LogError("❌ 并发安全性测试 FAILED - 数据不一致!");
            Debug.LogError($"期望：Counter=202, Message=Round2_Save2");
            Debug.LogError($"实际：Counter={finalCounter}, Message={finalMessage}");
        }

        Debug.Log("=== 🏁 并发安全性测试完成 ===");
    }

    [Button("Load All")]
    private void LoadAll()
    {
        KvSaveSystem.LoadAll(SaveSystemConst.PublicArchiveDirectoryPath);
        PrintSaveCacheData();
    }
    
    [Button("Clear Cache")]
    private void ClearAll()
    {
        KvSaveSystem.ClearCache();
        PrintSaveCacheData();
    }

    [Button("Print Save Cache Data")]
    public void PrintSaveCacheData()
    {
#if UNITY_EDITOR
        KvSaveSystem.PrintSaveCacheData();
#endif
    }
}