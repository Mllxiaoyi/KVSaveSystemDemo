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

public class WriteReadSpeedTest : MonoBehaviour
{
    [LabelText("测试数据量")]
    public int testCount = 10000;
    [LabelText("重复次数")]
    public int repeatTimes = 1;

    [Button("开始写入文件")]
    public void DoWrite()
    {
        var datas = UtilsForTest.GenerateTestKvPairListData(testCount);
        var dataCnt = datas.Count;
        string filePath = Path.Combine(Application.persistentDataPath, "WriteReadSpeedTest.txt");
        if (!File.Exists(filePath))
        {
            File.Create(filePath).Close();
        }

        using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Write))
            //using (CryptoStream csDecrypt = new CryptoStream(stream, KVSaveSystem.KVSaveSystem.AESEncryptor, CryptoStreamMode.Write))
        using (var streamWriter = new StreamWriter(stream, Encoding.UTF8))
        {
            streamWriter.AutoFlush = false;
            for (int k = 0; k <= dataCnt; k++)
            {
                string str = datas[k].ToString();
                streamWriter.WriteLine(str);
            }

            streamWriter.Flush();
        }
    }
    
    [Button("开始读取文件")]
    public void DoRead()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "WriteReadSpeedTest.txt");
        if (!File.Exists(filePath))
        {
            Debug.LogError("文件不存在，请先执行写入操作。");
            return;
        }

        Stopwatch stopwatch = Stopwatch.StartNew();
        using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        //using (CryptoStream csDecrypt = new CryptoStream(stream, KVSaveSystem.KVSaveSystem.GetAESDecryptor(stream), CryptoStreamMode.Read))
        using (var streamReader = new StreamReader(stream, Encoding.UTF8))
        {
            while (!streamReader.EndOfStream)
            {
                string line = streamReader.ReadLine();
                // 这里可以处理读取的行，例如解析成 KVPair 对象
                var kvPair = KVPair.FromString(line);
            }
        }
        
        stopwatch.Stop();
        Debug.Log($"读取文件耗时: {stopwatch.ElapsedMilliseconds} ms");
    }
}
