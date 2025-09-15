using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Sirenix.OdinInspector;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class AdditionalWriteSpeedTest : MonoBehaviour
{
    [LabelText("测试数据量")] public int testCount = 100;

    [LabelText("重复次数")] public int repeatTimes = 1;


    private const int keySize = 32; // AES-256

    private const int pwIterations = 100;

    private const string password = "jJf8HzFGg0Fg802A";

    private static ICryptoTransform _aesEncryptor;

    public static ICryptoTransform AESEncryptor
    {
        get
        {
            if (_aesEncryptor == null)
            {
                using (var alg = Aes.Create())
                {
                    alg.Mode = CipherMode.CBC;
                    alg.Padding = PaddingMode.PKCS7;
                    alg.GenerateIV();
                    var key = new Rfc2898DeriveBytes(password, alg.IV, pwIterations);
                    alg.Key = key.GetBytes(keySize);
                    _aesEncryptor = alg.CreateEncryptor();
                }
            }

            return _aesEncryptor;
        }
    }

    [Button("测试增量更新")]
    private void TestIncrementalUpdate()
    {
        var datas = UtilsForTest.GenerateTestKvPairListData(testCount);
        var dataCnt = datas.Count;
        string filePath = Path.Combine(Application.persistentDataPath, "tmp.txt");
        if (!File.Exists(filePath))
        {
            File.Create(filePath).Close();
        }

        List<long> times = new List<long>(repeatTimes);
        for (int i = 0; i < repeatTimes; i++)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            for (int j = 0; j < dataCnt; j++)
            {
                using (var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write))
                using (var streamWriter = new StreamWriter(stream, Encoding.UTF8))
                {
                    string str = datas[j].ToString();
                    streamWriter.WriteLine(str);
                }
            }

            stopwatch.Stop();
            times.Add(stopwatch.ElapsedMilliseconds);
        }

        Debug.Log($"Incremental Update: {times.Average()} ms");
    }

    [Button("测试全量更新")]
    private void TestTotalUpdate()
    {
        var datas = UtilsForTest.GenerateTestKvPairListData(testCount);
        var dataCnt = datas.Count;
        string filePath = Path.Combine(Application.persistentDataPath, "tmp.txt");
        if (!File.Exists(filePath))
        {
            File.Create(filePath).Close();
        }

        List<long> times = new List<long>(repeatTimes);
        for (int i = 0; i < repeatTimes; i++)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            for (int j = 0; j < dataCnt; j++)
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Write))
                using (var streamWriter = new StreamWriter(stream, Encoding.UTF8))
                {
                    streamWriter.AutoFlush = false;
                    for (int k = 0; k <= j; k++)
                    {
                        string str = datas[k].ToString();
                        streamWriter.WriteLine(str);
                    }

                    streamWriter.Flush();
                }
            }

            stopwatch.Stop();
            times.Add(stopwatch.ElapsedMilliseconds);
        }

        Debug.Log($"Total Update: {times.Average()} ms");
    }

    [Button("测试增量更新(加密)")]
    private void TestIncrementalUpdateEncrypted()
    {
        var datas = UtilsForTest.GenerateTestKvPairListData(testCount);
        var dataCnt = datas.Count;
        string filePath = Path.Combine(Application.persistentDataPath, "tmp_encrypted.txt");
        if (!File.Exists(filePath))
        {
            File.Create(filePath).Close();
        }

        List<long> times = new List<long>(repeatTimes);
        for (int i = 0; i < repeatTimes; i++)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            for (int j = 0; j < dataCnt; j++)
            {
                using (var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write))
                using (CryptoStream csDecrypt = new CryptoStream(stream, AESEncryptor, CryptoStreamMode.Write))
                using (var streamWriter = new StreamWriter(csDecrypt, Encoding.UTF8))
                {
                    string str = datas[j].ToString();
                    streamWriter.WriteLine(str);
                }
            }

            stopwatch.Stop();
            times.Add(stopwatch.ElapsedMilliseconds);
        }

        Debug.Log($"Incremental Update Encrypted: {times.Average()} ms");
    }

    [Button("测试全量更新(加密)")]
    private void TestTotalUpdateEncrypted()
    {
        var datas = UtilsForTest.GenerateTestKvPairListData(testCount);
        var dataCnt = datas.Count;
        string filePath = Path.Combine(Application.persistentDataPath, "tmp_encrypted.txt");
        if (!File.Exists(filePath))
        {
            File.Create(filePath).Close();
        }

        List<long> times = new List<long>(repeatTimes);
        for (int i = 0; i < repeatTimes; i++)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            for (int j = 0; j < dataCnt; j++)
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Write))
                using (CryptoStream csDecrypt = new CryptoStream(stream, AESEncryptor, CryptoStreamMode.Write))
                using (var streamWriter = new StreamWriter(csDecrypt, Encoding.UTF8))
                {
                    streamWriter.AutoFlush = false;
                    for (int k = 0; k <= j; k++)
                    {
                        string str = datas[k].ToString();
                        streamWriter.WriteLine(str);
                    }

                    streamWriter.Flush();
                }
            }

            stopwatch.Stop();
            times.Add(stopwatch.ElapsedMilliseconds);
        }

        Debug.Log($"Total Update Encrypted: {times.Average()} ms");
    }

    [Button("测试增量更新(ZeroFormatter + AES)")]
    private void TestIncrementalUpdateZeroFormatter()
    {
        var datas = UtilsForTest.GenerateTestKvPairListData(testCount);
        var dataCnt = datas.Count;
        string filePath = Path.Combine(Application.persistentDataPath, "tmp_zeroformatter.bin");
        if (!File.Exists(filePath))
        {
            File.Create(filePath).Close();
        }

        List<long> times = new List<long>(repeatTimes);
        for (int i = 0; i < repeatTimes; i++)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            for (int j = 0; j < dataCnt; j++)
            {
                using (var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write))
                using (CryptoStream csDecrypt = new CryptoStream(stream, AESEncryptor, CryptoStreamMode.Write))
                {
                    ZeroFormatter.ZeroFormatterSerializer.Serialize(csDecrypt, datas[j]);
                }
            }

            stopwatch.Stop();
            times.Add(stopwatch.ElapsedMilliseconds);
        }

        Debug.Log($"Incremental Update ZeroFormatter: {times.Average()} ms");
    }

    [Button("测试全量更新(ZeroFormatter + AES)")]
    private void TestTotalUpdateZeroFormatter()
    {
        IList<KVPair> datas = UtilsForTest.GenerateTestKvPairListData(testCount);
        var dataCnt = datas.Count;
        string filePath = Path.Combine(Application.persistentDataPath, "tmp_zeroformatter.bin");
        if (!File.Exists(filePath))
        {
            File.Create(filePath).Close();
        }

        List<long> times = new List<long>(repeatTimes);
        for (int i = 0; i < repeatTimes; i++)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            for (int j = 0; j < dataCnt; j++)
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Write))
                using (CryptoStream csDecrypt = new CryptoStream(stream, AESEncryptor, CryptoStreamMode.Write))
                {
                    for (int k = 0; k <= j; k++)
                    {
                        ZeroFormatter.ZeroFormatterSerializer.Serialize(csDecrypt, datas[k]);
                    }
                }
            }

            stopwatch.Stop();
            times.Add(stopwatch.ElapsedMilliseconds);
        }

        Debug.Log($"Total Update ZeroFormatter: {times.Average()} ms");
    }
}