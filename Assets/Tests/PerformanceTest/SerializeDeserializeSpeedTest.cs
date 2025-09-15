using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class SerializeDeserializeSpeedTest : MonoBehaviour
{
    [LabelText("测试数据量")]
    public int testCount = 10000;
    [LabelText("重复次数")]
    public int repeatTimes = 1;
    
    [Button("测试 XML 序列化")]
    public void TestXmlSerialization()
    {
        var data = UtilsForTest.GenerateTestKvPairListData(testCount);
        List<long> serializeTimes = new List<long>(repeatTimes);
        List<long> deserializeTimes = new List<long>(repeatTimes);
        
        for (int i = 0; i < repeatTimes; i++)
        {
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(List<KVPair>));
            using (var stream = new MemoryStream())
            {
                Stopwatch sw = Stopwatch.StartNew();
                serializer.Serialize(stream, data);
                sw.Stop();
                serializeTimes.Add(sw.ElapsedMilliseconds);
                stream.Position = 0;
                
                sw = Stopwatch.StartNew();
                var deserializedData = (List<KVPair>)serializer.Deserialize(stream);
                sw.Stop();
                deserializeTimes.Add(sw.ElapsedMilliseconds);
            }
        }
        
        UnityEngine.Debug.Log($"XML Serialization: {serializeTimes.Average()} ms. Deserialization: {deserializeTimes.Average()} ms with {testCount} items each.");
    }
    
    [Button("测试 JSON 序列化")]
    public void TestJsonSerialization()
    {
        var data = UtilsForTest.GenerateTestKvPairListData(testCount);
        List<long> serializeTimes = new List<long>(repeatTimes);
        List<long> deserializeTimes = new List<long>(repeatTimes);
        Dictionary<string, string> dic = new Dictionary<string, string>();
        foreach (var d in data)
        {
            dic.Add(d.Key, d.Value);
        }
        
        for (int i = 0; i < repeatTimes; i++)
        {
            Stopwatch sw = Stopwatch.StartNew();
            string json = JsonConvert.SerializeObject(dic);
            sw.Stop();
            serializeTimes.Add(sw.ElapsedMilliseconds);
            
            // 使用 ZeroFormatter 进行反序列化
            sw = Stopwatch.StartNew();
            var deserializedData = JsonConvert.DeserializeObject(json);
            sw.Stop();
            deserializeTimes.Add(sw.ElapsedMilliseconds);
        }
        
        UnityEngine.Debug.Log($"JSON Serialization: {serializeTimes.Average()} ms. Deserialization: {deserializeTimes.Average()} ms with {testCount} items each.");
    }
    
    [Button("测试 Binary 序列化")]
    public void TestBinarySerialization()
    {
        var data = UtilsForTest.GenerateTestKvPairListData(testCount);
        List<long> serializeTimes = new List<long>(repeatTimes);
        List<long> deserializeTimes = new List<long>(repeatTimes);
        
        for (int i = 0; i < repeatTimes; i++)
        {
            byte[] binaryData;
            Stopwatch sw = null;
            using (var memoryStream = new MemoryStream())
            {
                sw = Stopwatch.StartNew();
                var formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, data);
                sw.Stop();
                serializeTimes.Add(sw.ElapsedMilliseconds);
                binaryData = memoryStream.ToArray();
            }
            sw = Stopwatch.StartNew();
            var deserializedData = binaryData.Select(x => System.Text.Encoding.UTF8.GetString(new byte[] { x })).ToList();
            sw.Stop();
            deserializeTimes.Add(sw.ElapsedMilliseconds);
        }
        
        UnityEngine.Debug.Log($"Binary Serialization: {serializeTimes.Average()} ms. Deserialization: {deserializeTimes.Average()} ms with {testCount} items each.");
    }
    
    [Button("测试 ZeroFormatter 序列化")]
    public void TestZeroFormatterSerialization()
    {
        IList<KVPair> data = UtilsForTest.GenerateTestKvPairListData(testCount);
        List<long> serializeTimes = new List<long>(repeatTimes);
        List<long> deserializeTimes = new List<long>(repeatTimes);

        for (int i = 0; i < repeatTimes; i++)
        {
            // 使用 ZeroFormatter 进行序列化
            Stopwatch sw = Stopwatch.StartNew();
            byte[] zeroFormatterData = ZeroFormatter.ZeroFormatterSerializer.Serialize<IList<KVPair>>(data);
            sw.Stop();
            serializeTimes.Add(sw.ElapsedMilliseconds);
            
            // 使用 ZeroFormatter 进行反序列化
            sw = Stopwatch.StartNew();
            IList<KVPair> deserializedData = ZeroFormatter.ZeroFormatterSerializer.Deserialize<IList<KVPair>>(zeroFormatterData);
            sw.Stop();
            deserializeTimes.Add(sw.ElapsedMilliseconds);
        }
        
        UnityEngine.Debug.Log($"ZeroFormatter Serialization: {serializeTimes.Average()} ms. Deserialization: {deserializeTimes.Average()} ms with {testCount} items each.");
    }
    
    [Button("测试 string 自定义序列化")]
    public void TestStringCustomizedSerialization()
    {
        var data = UtilsForTest.GenerateTestKvPairListData(testCount);
        List<long> serializeTimes = new List<long>(repeatTimes);
        List<long> deserializeTimes = new List<long>(repeatTimes);
        
        for (int i = 0; i < repeatTimes; i++)
        {
            List<string> dataBuilder = new List<string>(testCount);
            Stopwatch sw = Stopwatch.StartNew();
            for (int j = 0; j < data.Count; j++)
            {
                string serializedData = data.ToString();
                dataBuilder.Add(serializedData);
            }
            sw.Stop();
            serializeTimes.Add(sw.ElapsedMilliseconds);
            
            sw = Stopwatch.StartNew();
            for (int j = 0; j < data.Count; j++)
            {
                string serializedData = dataBuilder[j];
                var deserializedData = KVPair.FromString(serializedData);
            }
            sw.Stop();
            deserializeTimes.Add(sw.ElapsedMilliseconds);
        }
        
        UnityEngine.Debug.Log($"Custom String Serialization: {serializeTimes.Average()} ms. Deserialization: {deserializeTimes.Average()} ms with {testCount} items each.");
    }

    // [Button("测试 Protobuf 序列化")]
    // public void TestProtobufSerialization()
    // {
    //     var data = UtilsForTest.GenerateTestKvPairListData(testCount);
    //     List<long> times = new List<long>(repeatTimes);
    //
    //     Stopwatch sw = Stopwatch.StartNew();
    //     for (int i = 0; i < repeatTimes; i++)
    //     {
    //         byte[] protobufData = ProtoBuf.Serializer.Serialize(data);
    //         var deserializedData = ProtoBuf.Serializer.Deserialize<List<KVPair>>(protobufData);
    //     }
    //     sw.Stop();
    //     times.Add(sw.ElapsedMilliseconds);
    //
    //     UnityEngine.Debug.Log($"Protobuf Serialization: {times.Average()} ms with {testCount} items each.");
    // }
    
    [Button("测试 Json + AES 序列化")]
    public void TestJsonAESSerialization()
    {
        var data = UtilsForTest.GenerateTestKvPairListData(testCount);
        List<long> serializeTimes = new List<long>(repeatTimes);
        List<long> deserializeTimes = new List<long>(repeatTimes);
        Dictionary<string, string> dic = new Dictionary<string, string>();
        foreach (var d in data)
        {
            dic.Add(d.Key, d.Value);
        }
        
        for (int i = 0; i < repeatTimes; i++)
        {
            Stopwatch sw = Stopwatch.StartNew();
            string json = JsonConvert.SerializeObject(dic);
            string encryptedData = ES3.EncryptString(json);
            byte[] encryptedBytes = Encoding.UTF8.GetBytes(encryptedData);
            sw.Stop();
            serializeTimes.Add(sw.ElapsedMilliseconds);

            sw = Stopwatch.StartNew();
            encryptedData = Encoding.UTF8.GetString(encryptedBytes);
            string decryptedData = ES3.DecryptString(encryptedData);
            var deserializedData = JsonConvert.DeserializeObject(decryptedData);
            sw.Stop();
            deserializeTimes.Add(sw.ElapsedMilliseconds);

        }
        
        UnityEngine.Debug.Log($"JSON + AES Serialization: {serializeTimes.Average()} ms. Deserialization: {deserializeTimes.Average()} ms with {testCount} items each.");
    }
    
    
    [Button("测试 ZeroFormatter + AES 序列化")]
    public void TestZeroFormatterAESSerialization()
    {
        var data = UtilsForTest.GenerateTestKvPairListData(testCount);
        List<long> times = new List<long>(repeatTimes);
        Dictionary<string, string> dic = new Dictionary<string, string>();
        foreach (var d in data)
        {
            dic.Add(d.Key, d.Value);
        }
        
        Stopwatch sw = Stopwatch.StartNew();
        for (int i = 0; i < repeatTimes; i++)
        {
            byte[] zeroFormatterData = AdvancedBinarySerializer.Serialize(dic, AESKey);
            var deserializedData = AdvancedBinarySerializer.Deserialize<Dictionary<string, string>>(zeroFormatterData, AESKey);
        }
        sw.Stop();
        times.Add(sw.ElapsedMilliseconds);
        
        UnityEngine.Debug.Log($"ZeroFormatter Serialization: {times.Average()} ms with {testCount} items each.");
    }
    
    byte[] AESKey = new byte[]{ 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
        0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10
    };
    
    public static class AdvancedBinarySerializer
    {
        private static readonly Aes aes = Aes.Create();
    
        // 带加密的序列化
        public static byte[] Serialize<T>(T obj, byte[] key)
        {
            try 
            {
                byte[] data = ZeroFormatter.ZeroFormatterSerializer.Serialize(obj);
                using (MemoryStream ms = new MemoryStream())
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(key, key), CryptoStreamMode.Write))
                {
                    cs.Write(data, 0, data.Length);
                    cs.FlushFinalBlock();
                    return ms.ToArray();
                }
            }
            catch (CryptographicException ex)
            {
                throw new SerializationException("Encryption failed", ex);
            }
        }

        // 带校验的反序列化
        public static T Deserialize<T>(byte[] encryptedData, byte[] key)
        {
            try 
            {
                using (MemoryStream ms = new MemoryStream(encryptedData))
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(key, key), CryptoStreamMode.Read))
                using (MemoryStream output = new MemoryStream())
                {
                    cs.CopyTo(output);
                    byte[] decrypted = output.ToArray();
                    return ZeroFormatter.ZeroFormatterSerializer.Deserialize<T>(decrypted);
                }
            }
            catch (Exception ex) when (ex is CryptographicException)
            {
                throw new SerializationException("Decryption or deserialization failed", ex);
            }
        }
    }
    
    
    
    
    
    
    
    [Button("测试 KVSaveSystem 保存")]
    public void TestSaveSystemSerialization()
    {
        var data = UtilsForTest.GenerateTestKvPairListData(testCount);
        List<long> times = new List<long>(repeatTimes);
        
        for (int i = 0; i < repeatTimes; i++)
        {
            KVSaveSystem.KvSaveSystem.ClearCache();
            foreach (var kv in data)
            {
                KVSaveSystem.KvSaveSystem.SetString(kv.Key, kv.Value);
            }
        }
        
        Stopwatch sw = Stopwatch.StartNew();
        for (int i = 0; i < repeatTimes; i++)
        {
            KVSaveSystem.KvSaveSystem.SaveAsync();
            //var deserializedData = KVSaveSystem.KVSaveSystem.LoadAllAsync();
        }
        sw.Stop();
        times.Add(sw.ElapsedMilliseconds);
        
        UnityEngine.Debug.Log($"KVSaveSystem Serialization: {times.Average()} ms with {testCount} items each.");
    }
    
    
    [Button("测试 Json 与 ZeroFormat 序列化")]
    public void TestJsonZeroFormatterSerialization()
    {
        var data = UtilsForTest.GenerateTestKvPairListData(testCount);
        string json = JsonConvert.SerializeObject(data);
        List<long> times = new List<long>(repeatTimes);
        
        string filePath = Application.persistentDataPath + "/test.json";
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        
        Stopwatch sw = Stopwatch.StartNew();
        byte[] dataBuffer = AdvancedBinarySerializer.Serialize(json, AESKey);
        using (var fs = new FileStream("async.bin", FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous))
        {
            fs.Write(dataBuffer, 0, 1);
        }

// StreamWriter异步
        using (var writer = new StreamWriter("async.txt", append: false, Encoding.UTF8, bufferSize: 8192))
        {
            writer.WriteLineAsync("异步文本");
        }
        Debug.Log($"KVSaveSystem Serialization: {times.Average()} ms with {testCount} items each.");
        
        
        sw = Stopwatch.StartNew();
        for (int i = 0; i < repeatTimes; i++)
        {
            KVSaveSystem.KvSaveSystem.SaveAsync();
            //var deserializedData = KVSaveSystem.KVSaveSystem.LoadAllAsync();
        }
        sw.Stop();
        times.Add(sw.ElapsedMilliseconds);
        
        UnityEngine.Debug.Log($"KVSaveSystem Serialization: {times.Average()} ms with {testCount} items each.");
    }
    
    
    [Button("测试 ZeroFormatter 写文件")]
    public void TestZeroFormatterWriteFile()
    {
        IList<KVPair> data = UtilsForTest.GenerateTestKvPairListData(testCount);
        string filePath = Application.persistentDataPath + "/test.bin";
        
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        
        Stopwatch sw = Stopwatch.StartNew();
        using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096))
        {
            byte[] zeroFormatterData = ZeroFormatter.ZeroFormatterSerializer.Serialize(data);
            fs.Write(zeroFormatterData, 0, zeroFormatterData.Length);
        }
        sw.Stop();
        
        UnityEngine.Debug.Log($"ZeroFormatter Write File: {sw.ElapsedMilliseconds} ms with {testCount} items each.");
    }
    
    [Button("测试自定义 String 写文件")]
    public void TestCustomStringWriteFile()
    {
        IList<KVPair> data = UtilsForTest.GenerateTestKvPairListData(testCount);
        string filePath = Application.persistentDataPath + "/test_custom_string.txt";
        
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        
        Stopwatch sw = Stopwatch.StartNew();
        using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096))
        {
            foreach (var kv in data)
            {
                string serializedData = kv.ToString();
                byte[] bytes = Encoding.UTF8.GetBytes(serializedData + Environment.NewLine);
                fs.Write(bytes, 0, bytes.Length);
            }
        }
        sw.Stop();
        
        UnityEngine.Debug.Log($"Custom String Write File: {sw.ElapsedMilliseconds} ms with {testCount} items each.");
    }
}
