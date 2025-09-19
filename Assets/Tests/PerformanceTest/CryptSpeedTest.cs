using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class CryptSpeedTest : MonoBehaviour
{

    private static readonly byte[] XorKey = { 0x5A, 0x3C, 0x7E, 0x12, 0x9F, 0x45, 0x67, 0x89 };
    private static readonly byte[] AesKey = {
        0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
        0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10
    };

    [Button("测试 XOR 混淆性能")]
    public void TestXorPerformance(int testCount, int repeatTimes)
    {
        var data = UtilsForTest.GenerateTestKvPairListData(testCount);
        string jsonData = JsonConvert.SerializeObject(data);
        byte[] originalBytes = Encoding.UTF8.GetBytes(jsonData);

        List<long> encryptTimes = new List<long>(repeatTimes);
        List<long> decryptTimes = new List<long>(repeatTimes);

        for (int i = 0; i < repeatTimes; i++)
        {
            Stopwatch sw = Stopwatch.StartNew();
            byte[] encryptedData = XorCrypt(originalBytes, XorKey);
            sw.Stop();
            encryptTimes.Add(sw.ElapsedTicks);

            sw = Stopwatch.StartNew();
            byte[] decryptedData = XorCrypt(encryptedData, XorKey);
            sw.Stop();
            decryptTimes.Add(sw.ElapsedTicks);
        }

        double avgEncryptMs = encryptTimes.Average() * 1000.0 / Stopwatch.Frequency;
        double avgDecryptMs = decryptTimes.Average() * 1000.0 / Stopwatch.Frequency;

        Debug.Log($"XOR 加密: {avgEncryptMs:F4} ms, 解密: {avgDecryptMs:F4} ms, 数据量: {originalBytes.Length} bytes, 重复: {repeatTimes} 次");
    }

    [Button("测试 AES 加密性能")]
    public void TestAesPerformance(int testCount, int repeatTimes)
    {
        var data = UtilsForTest.GenerateTestKvPairListData(testCount);
        string jsonData = JsonConvert.SerializeObject(data);
        byte[] originalBytes = Encoding.UTF8.GetBytes(jsonData);

        List<long> encryptTimes = new List<long>(repeatTimes);
        List<long> decryptTimes = new List<long>(repeatTimes);

        using (var aes = Aes.Create())
        {
            aes.Key = AesKey;
            aes.IV = AesKey;

            for (int i = 0; i < repeatTimes; i++)
            {
                Stopwatch sw = Stopwatch.StartNew();
                byte[] encryptedData = AesEncrypt(originalBytes, aes);
                sw.Stop();
                encryptTimes.Add(sw.ElapsedTicks);

                sw = Stopwatch.StartNew();
                byte[] decryptedData = AesDecrypt(encryptedData, aes);
                sw.Stop();
                decryptTimes.Add(sw.ElapsedTicks);
            }
        }

        double avgEncryptMs = encryptTimes.Average() * 1000.0 / Stopwatch.Frequency;
        double avgDecryptMs = decryptTimes.Average() * 1000.0 / Stopwatch.Frequency;

        Debug.Log($"AES 加密: {avgEncryptMs:F4} ms, 解密: {avgDecryptMs:F4} ms, 数据量: {originalBytes.Length} bytes, 重复: {repeatTimes} 次");
    }

    [Button("对比 XOR vs AES 性能")]
    public void CompareXorVsAes(int testCount, int repeatTimes)
    {
        var data = UtilsForTest.GenerateTestKvPairListData(testCount);
        string jsonData = JsonConvert.SerializeObject(data);
        byte[] originalBytes = Encoding.UTF8.GetBytes(jsonData);

        Debug.Log($"开始对比测试 - 数据量: {originalBytes.Length} bytes, 测试项: {testCount}, 重复: {repeatTimes} 次");

        // XOR 测试
        List<long> xorEncryptTimes = new List<long>(repeatTimes);
        List<long> xorDecryptTimes = new List<long>(repeatTimes);

        for (int i = 0; i < repeatTimes; i++)
        {
            Stopwatch sw = Stopwatch.StartNew();
            byte[] encryptedData = XorCrypt(originalBytes, XorKey);
            sw.Stop();
            xorEncryptTimes.Add(sw.ElapsedTicks);

            sw = Stopwatch.StartNew();
            byte[] decryptedData = XorCrypt(encryptedData, XorKey);
            sw.Stop();
            xorDecryptTimes.Add(sw.ElapsedTicks);
        }

        // AES 测试
        List<long> aesEncryptTimes = new List<long>(repeatTimes);
        List<long> aesDecryptTimes = new List<long>(repeatTimes);

        using (var aes = Aes.Create())
        {
            aes.Key = AesKey;
            aes.IV = AesKey;

            for (int i = 0; i < repeatTimes; i++)
            {
                Stopwatch sw = Stopwatch.StartNew();
                byte[] encryptedData = AesEncrypt(originalBytes, aes);
                sw.Stop();
                aesEncryptTimes.Add(sw.ElapsedTicks);

                sw = Stopwatch.StartNew();
                byte[] decryptedData = AesDecrypt(encryptedData, aes);
                sw.Stop();
                aesDecryptTimes.Add(sw.ElapsedTicks);
            }
        }

        // 计算平均时间
        double xorEncryptMs = xorEncryptTimes.Average() * 1000.0 / Stopwatch.Frequency;
        double xorDecryptMs = xorDecryptTimes.Average() * 1000.0 / Stopwatch.Frequency;
        double aesEncryptMs = aesEncryptTimes.Average() * 1000.0 / Stopwatch.Frequency;
        double aesDecryptMs = aesDecryptTimes.Average() * 1000.0 / Stopwatch.Frequency;

        // 计算速度倍数
        double encryptSpeedRatio = aesEncryptMs / xorEncryptMs;
        double decryptSpeedRatio = aesDecryptMs / xorDecryptMs;

        Debug.Log("=== XOR vs AES 性能对比结果 ===");
        Debug.Log($"XOR  - 加密: {xorEncryptMs:F4} ms, 解密: {xorDecryptMs:F4} ms");
        Debug.Log($"AES  - 加密: {aesEncryptMs:F4} ms, 解密: {aesDecryptMs:F4} ms");
        Debug.Log($"XOR 比 AES 快 {encryptSpeedRatio:F1} 倍 (加密), {decryptSpeedRatio:F1} 倍 (解密)");
    }

    [Button("XOR 流式 vs 整体处理对比")]
    public void CompareXorStreamVsBatch(int testCount, int repeatTimes)
    {
        var data = UtilsForTest.GenerateTestKvPairListData(testCount);
        string jsonData = JsonConvert.SerializeObject(data);
        byte[] originalBytes = Encoding.UTF8.GetBytes(jsonData);

        string streamFilePath = Path.Combine(Application.persistentDataPath, "test_xor_stream.dat");
        string batchFilePath = Path.Combine(Application.persistentDataPath, "test_xor_batch.dat");

        Debug.Log($"XOR 流式 vs 整体处理对比 - 数据量: {originalBytes.Length} bytes, 重复: {repeatTimes} 次");

        // 流式处理测试
        List<long> streamTimes = new List<long>(repeatTimes);
        for (int i = 0; i < repeatTimes; i++)
        {
            Stopwatch sw = Stopwatch.StartNew();

            // 流式加密写入
            using (var fileStream = new FileStream(streamFilePath, FileMode.Create, FileAccess.Write))
            using (var xorTransform = new XorTransform(XorKey))
            using (var cryptoStream = new CryptoStream(fileStream, xorTransform, CryptoStreamMode.Write))
            {
                cryptoStream.Write(originalBytes, 0, originalBytes.Length);
                cryptoStream.FlushFinalBlock();
            }

            // 流式解密读取
            using (var fileStream = new FileStream(streamFilePath, FileMode.Open, FileAccess.Read))
            using (var xorTransform = new XorTransform(XorKey))
            using (var cryptoStream = new CryptoStream(fileStream, xorTransform, CryptoStreamMode.Read))
            using (var output = new MemoryStream())
            {
                cryptoStream.CopyTo(output);
                byte[] decrypted = output.ToArray();
            }

            sw.Stop();
            streamTimes.Add(sw.ElapsedTicks);
        }

        // 整体处理测试
        List<long> batchTimes = new List<long>(repeatTimes);
        for (int i = 0; i < repeatTimes; i++)
        {
            Stopwatch sw = Stopwatch.StartNew();

            // 整体加密后写入
            byte[] encrypted = XorCrypt(originalBytes, XorKey);
            using (var fileStream = new FileStream(batchFilePath, FileMode.Create, FileAccess.Write))
            {
                fileStream.Write(encrypted, 0, encrypted.Length);
            }

            // 整体读取后解密
            using (var fileStream = new FileStream(batchFilePath, FileMode.Open, FileAccess.Read))
            {
                byte[] fromFile = new byte[fileStream.Length];
                fileStream.Read(fromFile, 0, fromFile.Length);
                byte[] decrypted = XorCrypt(fromFile, XorKey);
            }

            sw.Stop();
            batchTimes.Add(sw.ElapsedTicks);
        }

        // 计算平均时间
        double streamMs = streamTimes.Average() * 1000.0 / Stopwatch.Frequency;
        double batchMs = batchTimes.Average() * 1000.0 / Stopwatch.Frequency;
        double speedRatio = streamMs / batchMs;

        Debug.Log("=== XOR 流式 vs 整体处理对比结果 ===");
        Debug.Log($"流式处理: {streamMs:F4} ms");
        Debug.Log($"整体处理: {batchMs:F4} ms");
        Debug.Log($"性能差异: {(speedRatio > 1 ? "整体" : "流式")}处理快 {Math.Max(speedRatio, 1/speedRatio):F2} 倍");

        // 清理文件
        if (File.Exists(streamFilePath)) File.Delete(streamFilePath);
        if (File.Exists(batchFilePath)) File.Delete(batchFilePath);
    }

    [Button("文件读写性能对比")]
    public void CompareFileIOPerformance(int testCount, int repeatTimes)
    {
        var data = UtilsForTest.GenerateTestKvPairListData(testCount);
        string jsonData = JsonConvert.SerializeObject(data);
        byte[] originalBytes = Encoding.UTF8.GetBytes(jsonData);

        string xorFilePath = Path.Combine(Application.persistentDataPath, "test_xor.dat");
        string aesFilePath = Path.Combine(Application.persistentDataPath, "test_aes.dat");

        Debug.Log($"文件读写性能对比 - 数据量: {originalBytes.Length} bytes");

        // XOR 文件测试 - 使用 CryptoStream
        Stopwatch sw = Stopwatch.StartNew();
        using (var fileStream = new FileStream(xorFilePath, FileMode.Create, FileAccess.Write))
        using (var xorTransform = new XorTransform(XorKey))
        using (var cryptoStream = new CryptoStream(fileStream, xorTransform, CryptoStreamMode.Write))
        {
            cryptoStream.Write(originalBytes, 0, originalBytes.Length);
            cryptoStream.FlushFinalBlock();
        }

        using (var fileStream = new FileStream(xorFilePath, FileMode.Open, FileAccess.Read))
        using (var xorTransform = new XorTransform(XorKey))
        using (var cryptoStream = new CryptoStream(fileStream, xorTransform, CryptoStreamMode.Read))
        using (var output = new MemoryStream())
        {
            cryptoStream.CopyTo(output);
            byte[] xorDecrypted = output.ToArray();
        }
        sw.Stop();
        long xorFileTime = sw.ElapsedMilliseconds;

        // AES 文件测试
        sw = Stopwatch.StartNew();
        using (var aes = Aes.Create())
        {
            aes.Key = AesKey;
            aes.IV = AesKey;

            // 直接流写入
            using (var fileStream = new FileStream(aesFilePath, FileMode.Create, FileAccess.Write))
            using (var cryptoStream = new CryptoStream(fileStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cryptoStream.Write(originalBytes, 0, originalBytes.Length);
                cryptoStream.FlushFinalBlock();
            }

            // 直接流读取
            using (var fileStream = new FileStream(aesFilePath, FileMode.Open, FileAccess.Read))
            using (var cryptoStream = new CryptoStream(fileStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
            using (var output = new MemoryStream())
            {
                cryptoStream.CopyTo(output);
                byte[] aesDecrypted = output.ToArray();
            }
        }
        sw.Stop();
        long aesFileTime = sw.ElapsedMilliseconds;

        // 获取文件大小
        var xorFileInfo = new FileInfo(xorFilePath);
        var aesFileInfo = new FileInfo(aesFilePath);

        Debug.Log($"XOR 文件操作: {xorFileTime} ms, 文件大小: {xorFileInfo.Length} bytes");
        Debug.Log($"AES 文件操作: {aesFileTime} ms, 文件大小: {aesFileInfo.Length} bytes");
        Debug.Log($"XOR 比 AES 快 {(double)aesFileTime / xorFileTime:F1} 倍");

        // 清理文件
        // if (File.Exists(xorFilePath)) File.Delete(xorFilePath);
        // if (File.Exists(aesFilePath)) File.Delete(aesFilePath);
    }

    private static byte[] XorCrypt(byte[] data, byte[] key)
    {
        byte[] result = new byte[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            result[i] = (byte)(data[i] ^ key[i % key.Length]);
        }
        return result;
    }

    public class XorTransform : ICryptoTransform
    {
        private readonly byte[] _key;
        private int _keyIndex;

        public XorTransform(byte[] key)
        {
            _key = new byte[key.Length];
            Array.Copy(key, _key, key.Length);
            _keyIndex = 0;
        }

        public bool CanReuseTransform => false;
        public bool CanTransformMultipleBlocks => true;
        public int InputBlockSize => 1;
        public int OutputBlockSize => 1;

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            for (int i = 0; i < inputCount; i++)
            {
                outputBuffer[outputOffset + i] = (byte)(inputBuffer[inputOffset + i] ^ _key[_keyIndex % _key.Length]);
                _keyIndex++;
            }
            return inputCount;
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            byte[] output = new byte[inputCount];
            TransformBlock(inputBuffer, inputOffset, inputCount, output, 0);
            return output;
        }

        public void Dispose()
        {
            Array.Clear(_key, 0, _key.Length);
        }
    }

    private static byte[] AesEncrypt(byte[] data, Aes aes)
    {
        using (var encryptor = aes.CreateEncryptor())
        using (var ms = new MemoryStream())
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        {
            cs.Write(data, 0, data.Length);
            cs.FlushFinalBlock();
            return ms.ToArray();
        }
    }

    private static byte[] AesDecrypt(byte[] encryptedData, Aes aes)
    {
        using (var decryptor = aes.CreateDecryptor())
        using (var ms = new MemoryStream(encryptedData))
        using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
        using (var output = new MemoryStream())
        {
            cs.CopyTo(output);
            return output.ToArray();
        }
    }
}
