using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

public class IOSerializePlayerPrefsFirstTest : MonoBehaviour
{
    [LabelText("测试数据量")] 
    public int testCount = 10000;
    [LabelText("重复次数")] 
    public int repeatTimes = 1;
    [LabelText("测试文件名称")] 
    public string testFilePath = "IOSerializePlayerPrefsFirstTest.xml";

    [LabelText("文件保存路径")]
    [ShowInInspector]
    public string SaveFilePath => Path.Combine(Application.persistentDataPath, testFilePath);

    [Button("开始测试 PlayerPrefs、I/O+XML、Json 和 Dictionary的写入速度")]
    public void StartTest()
    {
        UnityEngine.Debug.Log("-----测试 PlayerPrefs、I/O+XML、Json 和 Dictionary的写入速度------");
        var kvList = UtilsForTest.GenerateTestKvPairListData(testCount);
        
        Stopwatch stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < repeatTimes; i++)
        {
            for (int j = 0; j < testCount; j++)
            {
                var kv = kvList[j];
                PlayerPrefs.SetString(kv.Key, kv.Value);
            }
            PlayerPrefs.Save();
        }
        stopwatch.Stop();
        UnityEngine.Debug.Log($"PlayerPrefs: {stopwatch.ElapsedMilliseconds}");


        XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<KVPair>));
        stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < repeatTimes; i++)
        {
            using (var stream = new FileStream(SaveFilePath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                stream.SetLength(0);
                xmlSerializer.Serialize(stream, kvList);
            }
        }
        stopwatch.Stop();
        UnityEngine.Debug.Log($"XML Serialize: {stopwatch.ElapsedMilliseconds}");

        stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < repeatTimes; i++)
        {
            using (var stream = new FileStream(SaveFilePath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                stream.SetLength(0);
                string json = JsonConvert.SerializeObject(kvList);
                stream.Write(System.Text.Encoding.UTF8.GetBytes(json), 0, json.Length);
            }
        }
        stopwatch.Stop();
        UnityEngine.Debug.Log($"Json Serialize: {stopwatch.ElapsedMilliseconds}");

        Dictionary<string, string> dic = new Dictionary<string, string>();
        stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < repeatTimes; i++)
        {
            for (int j = 0; j < testCount; j++)
            {
                var kv = kvList[j];
                // 考虑扩容
                dic[kv.Key] = kv.Value;
            }
        }
        stopwatch.Stop();
        UnityEngine.Debug.Log($"Dictionary: {stopwatch.ElapsedMilliseconds}");
    }


    [Button("开始XML写入分段耗时测试")]
    public void DoWriteSpeedTestXML()
    {
        var datas = UtilsForTest.GenerateTestKvPairListData(testCount);

        // 确保文件存在
        if (!File.Exists(SaveFilePath))
        {
            File.Create(SaveFilePath).Dispose();
        }

        XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<KVPair>));

        GC.Collect();

        // 测试 1: I/O + 序列化 + 加密
        Stopwatch stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < repeatTimes; i++)
        {
            using (FileStream fileStream = new FileStream(SaveFilePath, FileMode.Open, FileAccess.Write))
            using (var alg = Aes.Create())
            {
                alg.Mode = CipherMode.CBC;
                alg.Padding = PaddingMode.PKCS7;
                alg.GenerateIV();
                var key = new Rfc2898DeriveBytes("TEST26", alg.IV, 100);
                alg.Key = key.GetBytes(32);

                using (CryptoStream csEncrypt =
                       new CryptoStream(fileStream, alg.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    xmlSerializer.Serialize(csEncrypt, datas);
                }
            }
        }

        stopwatch.Stop();
        UnityEngine.Debug.Log($"I/O + 序列化写入 + 加密时间: {stopwatch.ElapsedMilliseconds} ms");

        // 测试 2: I/O + 序列化
        xmlSerializer = new XmlSerializer(typeof(List<KVPair>));
        stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < repeatTimes; i++)
        {
            using (FileStream fileStream = new FileStream(SaveFilePath, FileMode.Open, FileAccess.Write))
            {
                xmlSerializer.Serialize(fileStream, datas);
            }
        }

        stopwatch.Stop();
        UnityEngine.Debug.Log($"I/O + 序列化写入时间: {stopwatch.ElapsedMilliseconds} ms");

        // 测试 3: 内存流序列化
        xmlSerializer = new XmlSerializer(typeof(List<KVPair>));
        stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < repeatTimes; i++)
        {
            using (var memoryStream = new MemoryStream())
            {
                xmlSerializer.Serialize(memoryStream, datas);
            }
        }

        stopwatch.Stop();
        UnityEngine.Debug.Log($"内存流序列化时间: {stopwatch.ElapsedMilliseconds} ms");

        // 测试 4: 纯字节流写入
        xmlSerializer = new XmlSerializer(typeof(List<KVPair>));
        byte[] serializedData;
        using (var memoryStream = new MemoryStream())
        {
            xmlSerializer.Serialize(memoryStream, datas);
            serializedData = memoryStream.ToArray();
        }

        using (var memoryStream = new MemoryStream())
        {
            stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < repeatTimes; i++)
            {
                memoryStream.SetLength(0);
                memoryStream.Write(serializedData, 0, serializedData.Length);
            }

            stopwatch.Stop();
        }

        UnityEngine.Debug.Log($"纯字节流写入时间: {stopwatch.ElapsedMilliseconds} ms");
        
        // 测试 5: 直接字符串拼接序列化
        stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < repeatTimes; i++)
        {
            using (var memoryStream = new MemoryStream())
            using (StreamWriter writer = new StreamWriter(memoryStream))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<ListOfKVPair>");
                for (int j = 0; j < datas.Count; j++)
                {
                    sb.Append("<KVPair>");
                    sb.Append("<Key>").Append(System.Security.SecurityElement.Escape(datas[j].Key)).Append("</Key>");
                    sb.Append("<Value>").Append(System.Security.SecurityElement.Escape(datas[j].Value)).Append("</Value>");
                    sb.Append("</KVPair>");
                }
                sb.Append("</ListOfKVPair>");
                writer.Write(sb.ToString());
            }
        }
        stopwatch.Stop();
        UnityEngine.Debug.Log($"直接字符串拼接序列化时间: {stopwatch.ElapsedMilliseconds} ms");
        
    }


    [Button("开始Json写入分段耗时测试")]
    public void DoWriteSpeedTestJson()
    {
        var datas = UtilsForTest.GenerateTestKvPairListData(testCount);

        // 确保文件存在
        if (!File.Exists(SaveFilePath))
        {
            File.Create(SaveFilePath).Dispose();
        }


        GC.Collect();

        // 测试 1: I/O + 序列化 + 加密
        Stopwatch stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < repeatTimes; i++)
        {
            using (FileStream fileStream = new FileStream(SaveFilePath, FileMode.Open, FileAccess.Write))
            using (var alg = Aes.Create())
            {
                alg.Mode = CipherMode.CBC;
                alg.Padding = PaddingMode.PKCS7;
                alg.GenerateIV();
                var key = new Rfc2898DeriveBytes("TEST26", alg.IV, 100);
                alg.Key = key.GetBytes(32);

                using (CryptoStream csEncrypt =
                       new CryptoStream(fileStream, alg.CreateEncryptor(), CryptoStreamMode.Write))
                using (StreamWriter writer = new StreamWriter(csEncrypt))
                {
                    string json = JsonConvert.SerializeObject(datas);
                    writer.Write(json);
                }
            }
        }

        stopwatch.Stop();
        UnityEngine.Debug.Log($"I/O + 序列化写入 + 加密时间: {stopwatch.ElapsedMilliseconds} ms");

        // 测试 2: I/O + 序列化
        stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < repeatTimes; i++)
        {
            using (FileStream fileStream = new FileStream(SaveFilePath, FileMode.Open, FileAccess.Write))
            using (StreamWriter writer = new StreamWriter(fileStream))
            {
                string json = JsonConvert.SerializeObject(datas);
                writer.Write(json);
            }
        }

        stopwatch.Stop();
        UnityEngine.Debug.Log($"I/O + 序列化写入时间: {stopwatch.ElapsedMilliseconds} ms");

        // 测试 3: 内存流序列化
        stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < repeatTimes; i++)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(memoryStream))
                {
                    string json = JsonConvert.SerializeObject(datas);
                    writer.Write(json);
                }
            }
        }

        stopwatch.Stop();
        UnityEngine.Debug.Log($"内存流序列化时间: {stopwatch.ElapsedMilliseconds} ms");

        // 测试 4: 纯字节流写入
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(datas));
        stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < repeatTimes; i++)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(memoryStream))
                {
                    writer.Write(bytes);
                }
            }
        }

        stopwatch.Stop();
        UnityEngine.Debug.Log($"纯字节流写入时间: {stopwatch.ElapsedMilliseconds} ms");
    }
}