using System.Collections;
using System.IO;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using Debug = UnityEngine.Debug;

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
        KvSaveSystem.LoadAll(KvSaveSystemConst.PublicArchiveDirectoryPath);
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
        KvSaveSystem.SaveAsyncInternal(true);
    }

    [Button("SaveMultiTimesInSingleFrame")]
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

    private long callTimes = 0;

    [Button("MultiSave")]
    private void MultiSave()
    {
        var repeatTimes = 10000;
        for (int i = 0; i < repeatTimes; i++)
        {
            callTimes = callTimes + 1;
            Task.Run(async () =>
            {
                var tmpPath = KvSaveSystemConst.GetGroupFilePath("MultiSave" + callTimes);
                var finalPath = tmpPath + ".fin";

                using (Stream stream = new FileStream(tmpPath, FileMode.Create, FileAccess.Write))
                {
                    var bytes = new byte[] { 1 };
                    await stream.WriteAsync(bytes, 0, bytes.Length);
                    await Task.Delay(200);
                    await stream.FlushAsync();
                }
                
                if (File.Exists(finalPath))
                    File.Delete(finalPath);
                await Task.Delay(500);
                File.Copy(tmpPath, finalPath);
            });
        }
    }

    [Button("Load Multi Data")]
    public void LoadMultiData()
    {
        KvSaveSystem.LoadAll(KvSaveSystemConst.PublicArchiveDirectoryPath);
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
        KvSaveSystem.LoadAll(KvSaveSystemConst.PublicArchiveDirectoryPath);

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

    [Button("测试加载全部")]
    private void LoadAll()
    {
        KvSaveSystem.LoadAll(KvSaveSystemConst.PublicArchiveDirectoryPath);
    }

#if UNITY_EDITOR
    [Button("Print Save Cache Data")]
    public void PrintSaveCacheData()
    {
        KvSaveSystem.PrintSaveCacheData();
    }
#endif
}