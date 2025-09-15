using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using ES3Internal;
using Sirenix.OdinInspector;
using UnityEngine;

public class StreamByteWriteSpeedTest : MonoBehaviour
{
    public int repeatTimes = 10000;
    
    public string data = "This is a test string to measure the speed of writing to a byte stream. It will be repeated multiple times to ensure we have enough data to test the performance of the write operation.";
    
    [Button("测试 Stream 写入速度")]
    private void StartTest()
    {
        byte[] byteData = System.Text.Encoding.UTF8.GetBytes(data);

        // 使用MemoryStream避免磁盘I/O影响
        using var memoryStream = new MemoryStream(capacity: repeatTimes * byteData.Length * 2);

        // 测试byte[]写入
        var swBytes = Stopwatch.StartNew();
        for (int i = 0; i < repeatTimes; i++)
        {
            memoryStream.Write(byteData, 0, byteData.Length);
        }
        swBytes.Stop();
        memoryStream.SetLength(0);

        // 测试string写入（先编码为byte[]）
        var swString = Stopwatch.StartNew();
        for (int i = 0; i < repeatTimes; i++)
        {
            byte[] encoded = Encoding.UTF8.GetBytes(data);
            memoryStream.Write(encoded, 0, encoded.Length);
        }
        swString.Stop();
        memoryStream.SetLength(0);

        // 测试StreamWriter写入
        Stopwatch swWriter = null;
        using (var writer = new StreamWriter(memoryStream, Encoding.UTF8))
        {
            swWriter = Stopwatch.StartNew();
            for (int i = 0; i < repeatTimes; i++)
            {
                writer.Write(data);
            }
            swWriter.Stop();
        }

        // 打印结果
        UnityEngine.Debug.Log($"byte[] 写入耗时: {swBytes.ElapsedMilliseconds} ms");
        UnityEngine.Debug.Log($"string→byte[] 写入耗时: {swString.ElapsedMilliseconds} ms");
        UnityEngine.Debug.Log($"StreamWriter 写入耗时: {swWriter.ElapsedMilliseconds} ms");
    }
    
    [Button("测试 Stream + 加密 写入速度")]
    private void StartTestWithEncryption()
    {
        byte[] byteData = System.Text.Encoding.UTF8.GetBytes(data);
        var encryptionAlgorithm = new AESEncryptionAlgorithm(); // 假设你有一个加密算法类

        // 使用MemoryStream避免磁盘I/O影响
        using var memoryStream = new MemoryStream(capacity: repeatTimes * byteData.Length * 2);

        // 测试加密后的byte[]写入
        var swBytes = Stopwatch.StartNew();
        for (int i = 0; i < repeatTimes; i++)
        {
            byte[] encryptedData = encryptionAlgorithm.Encrypt(byteData, "password", 1024);
            memoryStream.Write(encryptedData, 0, encryptedData.Length);
        }
        swBytes.Stop();
        
        // 测试加密后StreamWriter写入
        Stopwatch swWriter = null;
        using (var writer = new StreamWriter(memoryStream, Encoding.UTF8))
        {
            swWriter = Stopwatch.StartNew();
            for (int i = 0; i < repeatTimes; i++)
            {
                byte[] encryptedData = encryptionAlgorithm.Encrypt(byteData, "password", 1024);
                writer.Write(encryptedData);
            }
            swWriter.Stop();
        }
        
        // 打印结果
        UnityEngine.Debug.Log($"加密后的byte[] 写入耗时: {swBytes.ElapsedMilliseconds} ms");
        UnityEngine.Debug.Log($"加密后的StreamWriter 写入耗时: {swWriter.ElapsedMilliseconds} ms");
    }
}
