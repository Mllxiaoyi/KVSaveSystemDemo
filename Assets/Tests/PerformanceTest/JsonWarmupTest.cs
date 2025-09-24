using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

public class JsonWarmupTest : MonoBehaviour
{
    [Button("测试 Json 预热效应")]
    public void TestJsonWarmupEffect()
    {
        var testData = UtilsForTest.GenerateTestKvPairListData(1000);

        UnityEngine.Debug.Log("=== Json 序列化预热效应测试 ===");

        // 测试多次序列化，观察性能变化
        List<long> times = new List<long>();

        for (int i = 0; i < 10; i++)
        {
            // 强制垃圾回收，确保测试环境一致
            if (i == 0)
            {
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                System.GC.Collect();
            }

            var sw = Stopwatch.StartNew();
            string jsonResult = JsonConvert.SerializeObject(testData);
            sw.Stop();

            times.Add(sw.ElapsedMilliseconds);

            UnityEngine.Debug.Log($"第 {i + 1} 次序列化: {sw.ElapsedMilliseconds} ms");
        }

        UnityEngine.Debug.Log($"首次序列化: {times[0]} ms");
        UnityEngine.Debug.Log($"平均后续: {times.Skip(1).Average():F2} ms");
        UnityEngine.Debug.Log($"性能提升: {(double)times[0] / times.Skip(1).Average():F1}x");
    }

    [Button("测试不同序列化器的预热")]
    public void TestDifferentSerializersWarmup()
    {
        var testData = UtilsForTest.GenerateTestKvPairListData(1000);

        UnityEngine.Debug.Log("=== 不同序列化器预热对比 ===");

        // 1. Unity JsonUtility
        TestSerializerWarmup("Unity JsonUtility",
            () => JsonUtility.ToJson(testData),
            testData);

        // 2. Newtonsoft.Json
        TestSerializerWarmup("Newtonsoft.Json",
            () => JsonConvert.SerializeObject(testData),
            testData);
    }

    private void TestSerializerWarmup<T>(string name, Func<string> serializer, T testData)
    {
        UnityEngine.Debug.Log($"\n--- {name} 预热测试 ---");

        List<long> times = new List<long>();

        // 强制垃圾回收
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
        System.GC.Collect();

        for (int i = 0; i < 5; i++)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                string result = serializer();
                sw.Stop();
                times.Add(sw.ElapsedMilliseconds);
                UnityEngine.Debug.Log($"{name} 第 {i + 1} 次: {sw.ElapsedMilliseconds} ms");
            }
            catch (Exception e)
            {
                sw.Stop();
                UnityEngine.Debug.LogError($"{name} 序列化失败: {e.Message}");
            }
        }

        if (times.Count > 1)
        {
            UnityEngine.Debug.Log($"{name} 首次: {times[0]} ms, 后续平均: {times.Skip(1).Average():F2} ms, 提升: {(double)times[0] / times.Skip(1).Average():F1}x");
        }
    }

    [Button("测试 JIT 预编译效果")]
    public void TestJitPrecompileEffect()
    {
        var smallData = UtilsForTest.GenerateTestKvPairListData(10);
        var largeData = UtilsForTest.GenerateTestKvPairListData(1000);

        UnityEngine.Debug.Log("=== JIT 预编译效果测试 ===");

        // 1. 先用小数据预热 JIT
        UnityEngine.Debug.Log("步骤1: 小数据预热 JIT");
        var sw = Stopwatch.StartNew();
        JsonConvert.SerializeObject(smallData);
        sw.Stop();
        UnityEngine.Debug.Log($"小数据预热耗时: {sw.ElapsedMilliseconds} ms");

        System.GC.Collect();

        // 2. 测试大数据序列化（JIT 已预热）
        UnityEngine.Debug.Log("步骤2: 大数据序列化 (JIT已预热)");
        List<long> warmedUpTimes = new List<long>();

        for (int i = 0; i < 3; i++)
        {
            sw = Stopwatch.StartNew();
            JsonConvert.SerializeObject(largeData);
            sw.Stop();
            warmedUpTimes.Add(sw.ElapsedMilliseconds);
            UnityEngine.Debug.Log($"预热后大数据第 {i + 1} 次: {sw.ElapsedMilliseconds} ms");
        }

        UnityEngine.Debug.Log($"预热后平均耗时: {warmedUpTimes.Average():F2} ms");

        // 3. 重新启动测试（模拟冷启动）
        UnityEngine.Debug.Log("步骤3: 对比 - 如果是冷启动会如何");
        UnityEngine.Debug.Log("(实际无法重新冷启动JIT，这里仅作对比参考)");
    }

    [Button("测试内存分配模式")]
    public void TestMemoryAllocationPattern()
    {
        var testData = UtilsForTest.GenerateTestKvPairListData(1000);

        UnityEngine.Debug.Log("=== 内存分配模式测试 ===");

        // 强制垃圾回收，获取基线
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
        System.GC.Collect();

        long startMemory = System.GC.GetTotalMemory(false);

        // 第一次序列化
        var sw = Stopwatch.StartNew();
        string result1 = JsonConvert.SerializeObject(testData);
        sw.Stop();

        long afterFirstMemory = System.GC.GetTotalMemory(false);
        long firstAllocation = afterFirstMemory - startMemory;

        UnityEngine.Debug.Log($"第一次序列化: {sw.ElapsedMilliseconds} ms, 内存分配: {firstAllocation / 1024f:F2} KB");

        // 第二次序列化
        long beforeSecondMemory = System.GC.GetTotalMemory(false);
        sw = Stopwatch.StartNew();
        string result2 = JsonConvert.SerializeObject(testData);
        sw.Stop();

        long afterSecondMemory = System.GC.GetTotalMemory(false);
        long secondAllocation = afterSecondMemory - beforeSecondMemory;

        UnityEngine.Debug.Log($"第二次序列化: {sw.ElapsedMilliseconds} ms, 内存分配: {secondAllocation / 1024f:F2} KB");

        UnityEngine.Debug.Log($"内存分配减少: {(firstAllocation - secondAllocation) / 1024f:F2} KB");
    }
}